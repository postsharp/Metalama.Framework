// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using System.Runtime.Caching;

namespace Metalama.Framework.Engine.Utilities;

internal static class CacheByFile<T>
{
    private static readonly MemoryCache _cache = new( typeof(CacheByFile<T>).ToString() );
    
    // ReSharper disable once StaticMemberInGenericType
    private static readonly CacheItemPolicy _cacheItemPolicy = new() { SlidingExpiration = TimeSpan.FromMinutes( 5 ) };

    public static T Get( string path, Func<string, T> func )
    {
        var lastWriteTime = File.GetLastWriteTime( path );
        var entry = (CacheEntry?) _cache.Get( path );

        if ( entry == null || lastWriteTime != entry.LastWriteTime )
        {
            var value = func( path );
            entry = new CacheEntry( lastWriteTime, value );
            _cache.Set( path, entry, _cacheItemPolicy );
        }

        return entry.Value;
    }

    private class CacheEntry
    {
        public DateTime LastWriteTime { get; }

        public T Value { get; }

        public CacheEntry( DateTime lastWriteTime, T value )
        {
            this.LastWriteTime = lastWriteTime;
            this.Value = value;
        }
    }
}