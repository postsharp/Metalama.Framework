// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Caravela.Framework.Impl.CompileTime
{
    internal static class MetadataReferenceCache
    {
        private static readonly ConcurrentDictionary<string, (MetadataReference Reference, DateTime LastWriteTime)> _cache =
            new( StringComparer.Ordinal );

        public static MetadataReference GetFromFile( string path )
        {
            var lastWriteTime = File.GetLastWriteTime( path );

            if ( !_cache.TryGetValue( path, out var cached ) || lastWriteTime > cached.LastWriteTime )
            {
                cached = (MetadataReference.CreateFromFile( path ), lastWriteTime);
                _cache[path] = cached;
            }

            return cached.Reference;
        }
    }
}