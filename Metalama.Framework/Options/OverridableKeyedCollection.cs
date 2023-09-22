// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Options;

#pragma warning disable SA1642

[CompileTime]
public static class OverridableKeyedCollection
{
    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the absence of any operation.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> Empty<TKey, TValue>()
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new( ImmutableDictionary<TKey, OverridableKeyedCollection<TKey, TValue>.Item>.Empty );

    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the operation of removing any item both in the overridden collection.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> Clear<TKey, TValue>()
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new( ImmutableDictionary<TKey, OverridableKeyedCollection<TKey, TValue>.Item>.Empty, true );

    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the operation of adding an item to
    /// the overridden collection, or to override with new value if this item already exists.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> AddOrOverride<TKey, TValue>( TValue item )
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new(
            ImmutableDictionary.Create<TKey, OverridableKeyedCollection<TKey, TValue>.Item>()
                .Add( item.Key, new OverridableKeyedCollection<TKey, TValue>.Item( item ) ) );

    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the operation of adding items to
    /// the overridden collection, or to override with new value if this item already exists.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> AddOrOverride<TKey, TValue>( params TValue[] items )
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new( items.ToImmutableDictionary( i => i.Key, i => new OverridableKeyedCollection<TKey, TValue>.Item( i ) ) );

    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the operation of adding items to
    /// the overridden collection, or to override with new value if this item already exists.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> AddOrOverride<TKey, TValue>( IEnumerable<TValue> items )
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new( items.ToImmutableDictionary( i => i.Key, i => new OverridableKeyedCollection<TKey, TValue>.Item( i ) ) );

    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the option of removing an item
    /// from the overridden collection.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> Remove<TKey, TValue>( TValue item )
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new(
            ImmutableDictionary.Create<TKey, OverridableKeyedCollection<TKey, TValue>.Item>()
                .Add( item.Key, new OverridableKeyedCollection<TKey, TValue>.Item( item, false ) ) );

    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the option of removing items
    /// from the overridden collection.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> Remove<TKey, TValue>( params TValue[] items )
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new( items.ToImmutableDictionary( i => i.Key, i => new OverridableKeyedCollection<TKey, TValue>.Item( i, false ) ) );

    /// <summary>
    /// Creates a <see cref="OverridableKeyedCollection{TKey,TValue}"/> that represents the option of removing items
    /// from the overridden collection.
    /// </summary>
    public static OverridableKeyedCollection<TKey, TValue> Remove<TKey, TValue>( IEnumerable<TValue> items )
        where TValue : class, IOverridableKeyedCollectionItem<TKey>
        where TKey : notnull
        => new( items.ToImmutableDictionary( i => i.Key, i => new OverridableKeyedCollection<TKey, TValue>.Item( i, false ) ) );
}