﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising;

internal sealed class LazyObjectReader : IObjectReader
{
    private readonly Lazy<IObjectReader> _lazyUnderlying;

    public LazyObjectReader( Lazy<IObjectReader> lazyUnderlying ) 
    {
        this._lazyUnderlying = lazyUnderlying;
    }

    private IObjectReader Underlying => this._lazyUnderlying.Value;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => this.Underlying.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) this.Underlying).GetEnumerator();

    public int Count => this.Underlying.Count;

    public bool ContainsKey( string key ) => this.Underlying.ContainsKey( key );

    public bool TryGetValue( string key, out object? value ) => this.Underlying.TryGetValue( key, out value );

    public object? this[ string key ] => this.Underlying[key];

    public IEnumerable<string> Keys => this.Underlying.Keys;

    public IEnumerable<object?> Values => this.Underlying.Values;

    public object? Source => this.Underlying.Source;
}

internal sealed class ObjectReaderFactory : IProjectService, IDisposable
{
    private readonly WeakCache<Type, ObjectReaderTypeAdapter> _types = new();

    private readonly ProjectServiceProvider _serviceProvider;

    public IObjectReader GetReader( object? instance )
        => instance switch
        {
            null => ObjectReader.Empty,
            IObjectReader objectReader => objectReader,
            IReadOnlyDictionary<string, object?> dictionary => new ObjectReaderDictionaryWrapper( dictionary ),
            _ => new ObjectReader( instance, this.GetTypeAdapter( instance.GetType() ) )
        };

    public IObjectReader GetLazyReader( object? instance1, Func<object?> getInstance2 )
        => new LazyObjectReader(
            new Lazy<IObjectReader>(
                () =>
                {
                    var instance2 = getInstance2();

                    return (instance1, instance2) switch
                    {
                        (not null, null) => this.GetReader( instance1 ),
                        (null, not null) => this.GetReader( instance2 ),
                        _ => new ObjectReaderMergeWrapper( this.GetReader( instance2 ), this.GetReader( instance1 ) )
                    };
                } ) );

    private ObjectReaderTypeAdapter GetTypeAdapter( Type type ) => this._types.GetOrAdd( type, t => new ObjectReaderTypeAdapter( this._serviceProvider, t ) );

    public ObjectReaderFactory( in ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }

    public void Dispose() => this._types.Dispose();
}