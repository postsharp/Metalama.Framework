// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.IO;

namespace Metalama.Framework.Engine.Utilities.Caching;

/// <summary>
/// A cache where the key is a file and the last write time of that file.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class FileBasedCache<T>
{
    private readonly FixedCapacityCache<CacheEntry> _cache = new( 500 );

    public T Get( string path, Func<string, T> func )
    {
        var lastWriteTime = File.GetLastWriteTime( path );

        return this._cache.GetOrAdd( path, e => e.LastWriteTime == lastWriteTime, e => new CacheEntry( lastWriteTime, func( e ) ) ).Value;
    }

    private readonly struct CacheEntry
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