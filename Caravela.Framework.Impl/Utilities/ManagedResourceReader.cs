// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Caravela.Framework.Impl.Utilities
{
    internal static class ManagedResourceReader
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> _resourceCache = new();

        private record CacheEntry( DateTime LastFileWrite, ImmutableDictionary<string, ImmutableArray<byte>>? Resources );

        public static bool TryGetCompileTimeResource( string path, [NotNullWhen( true )] out ImmutableDictionary<string, ImmutableArray<byte>>? resources )
        {
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
            ImmutableDictionary<string, ImmutableArray<byte>>.Builder? resourceBuilder = null;

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

                if ( resourceName.Contains( CompileTimeConstants.CompileTimeProjectResourceName ) )
                {
                    unsafe
                    {
                        resourceBuilder ??= ImmutableDictionary.CreateBuilder<string, ImmutableArray<byte>>();
                        var resourcesSection = peReader.GetSectionData( peReader.PEHeaders.CorHeader!.ResourcesDirectory.RelativeVirtualAddress );
                        var size = *(int*) (resourcesSection.Pointer + resource.Offset);
                        var resourceBytes = resourcesSection.GetContent( sizeof(int), size );

                        resourceBuilder.Add( resourceName, resourceBytes );
                    }
                }
            }

            return new CacheEntry( timestamp, resourceBuilder?.ToImmutable() );
        }
    }
}