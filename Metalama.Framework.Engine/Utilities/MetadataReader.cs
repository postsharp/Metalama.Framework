// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Metalama.Framework.Engine.Utilities
{
    internal static class MetadataReader
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> _resourceCache = new();

        private record CacheEntry( DateTime LastFileWrite, ImmutableDictionary<string, byte[]>? Resources, ImmutableArray<string> AssemblyReferences );

        public static bool TryGetMetadata(
            string path,
            [NotNullWhen( true )] out ImmutableDictionary<string, byte[]>? resources,
            out ImmutableArray<string> assemblyReferences )
        {
            var assemblyName = Path.GetFileNameWithoutExtension( path );

            if ( assemblyName.Equals( "System", StringComparison.OrdinalIgnoreCase ) ||
                 assemblyName.StartsWith( "System.", StringComparison.OrdinalIgnoreCase ) ||
                 assemblyName.StartsWith( "Microsoft.CodeAnalysis", StringComparison.OrdinalIgnoreCase ) )
            {
                resources = null;

                return false;
            }

            if ( !(_resourceCache.TryGetValue( path, out var result ) && result.LastFileWrite == File.GetLastWriteTime( path )) )
            {
                result = GetCompileTimeResourceCore( path );
                _resourceCache.TryAdd( path, result );
            }

            resources = result.Resources ?? ImmutableDictionary<string, byte[]>.Empty;
            assemblyReferences = result.AssemblyReferences;

            return true;
        }

        private static CacheEntry GetCompileTimeResourceCore( string path )
        {
            var timestamp = File.GetLastWriteTime( path );

            using var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            using var peReader = new PEReader( stream );

            var metadataReader = peReader.GetMetadataReader();

            // Read resources.
            ImmutableDictionary<string, byte[]>.Builder? resourcesDictionaryBuilder = null;

            foreach ( var resourceHandle in metadataReader.ManifestResources )
            {
                var resource = metadataReader.GetManifestResource( resourceHandle );

                if ( !resource.Implementation.IsNil )
                {
                    // Coverage: ignore
                    // (This happens in the case that the resource is stored in a different module of the assembly, but this is a very rare
                    // case that cannot be easily tested without creating custom IL.)

                    continue;
                }

                var resourceName = metadataReader.GetString( resource.Name );

                if ( resourceName is CompileTimeConstants.CompileTimeProjectResourceName or CompileTimeConstants.InheritableAspectManifestResourceName )
                {
                    unsafe
                    {
                        resourcesDictionaryBuilder ??= ImmutableDictionary.CreateBuilder<string, byte[]>();
                        var resourcesSection = peReader.GetSectionData( peReader.PEHeaders.CorHeader!.ResourcesDirectory.RelativeVirtualAddress );
                        var size = *(int*) (resourcesSection.Pointer + resource.Offset);
                        var resourceBytes = new byte[size];

                        fixed ( byte* fixedResourceBytes = resourceBytes )
                        {
                            Buffer.MemoryCopy( resourcesSection.Pointer + resource.Offset + 4, fixedResourceBytes, size, size );
                        }

                        resourcesDictionaryBuilder.Add( resourceName, resourceBytes );
                    }
                }
            }

            // Read references.
            var referenceArrayBuilder = ImmutableArray.CreateBuilder<string>();

            foreach ( var assemblyReferenceHandle in metadataReader.AssemblyReferences )
            {
                var reference = metadataReader.GetAssemblyReference( assemblyReferenceHandle );
                var name = metadataReader.GetString( reference.Name );
                referenceArrayBuilder.Add( name );
            }

            return new CacheEntry( timestamp, resourcesDictionaryBuilder?.ToImmutable(), referenceArrayBuilder.ToImmutableArray() );
        }
    }
}