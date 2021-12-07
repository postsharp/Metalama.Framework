// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.CompileTime;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Metalama.Framework.Impl.Utilities
{
    internal static class ManagedResourceReader
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> _resourceCache = new();

        private record CacheEntry( DateTime LastFileWrite, ImmutableDictionary<string, byte[]>? Resources );

        public static bool TryGetCompileTimeResource( string path, [NotNullWhen( true )] out ImmutableDictionary<string, byte[]>? resources )
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

            if ( result.Resources != null )
            {
                resources = result.Resources;

                return true;
            }
            else
            {
                resources = null;

                return false;
            }
        }

        private static CacheEntry GetCompileTimeResourceCore( string path )
        {
            var timestamp = File.GetLastWriteTime( path );

            using var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            using var peReader = new PEReader( stream );

            var metadataReader = peReader.GetMetadataReader();

            ImmutableDictionary<string, byte[]>.Builder? dictionaryBuilder = null;

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
                        dictionaryBuilder ??= ImmutableDictionary.CreateBuilder<string, byte[]>();
                        var resourcesSection = peReader.GetSectionData( peReader.PEHeaders.CorHeader!.ResourcesDirectory.RelativeVirtualAddress );
                        var size = *(int*) (resourcesSection.Pointer + resource.Offset);
                        var resourceBytes = new byte[size];

                        fixed ( byte* fixedResourceBytes = resourceBytes )
                        {
                            Buffer.MemoryCopy( resourcesSection.Pointer + resource.Offset + 4, fixedResourceBytes, size, size );
                        }

                        dictionaryBuilder.Add( resourceName, resourceBytes );
                    }
                }
            }

            return new CacheEntry( timestamp, dictionaryBuilder?.ToImmutable() );
        }
    }
}