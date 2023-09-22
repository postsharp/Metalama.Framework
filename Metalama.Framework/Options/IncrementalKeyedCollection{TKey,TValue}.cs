// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Options;

/// <summary>
/// An immutable keyed collection where each class instance does not represent the full collection but a modification of another collection (possibly empty).
/// This class implements the <see cref="IOverridable"/> semantic and can be easily used in the context of an <see cref="IHierarchicalOptions{T}"/>.
/// The class can represent the <see cref="AddOrOverride(TValue)"/>, <see cref="Remove(TKey)"/> and <see cref="Clear"/> operations.
/// </summary>
/// <typeparam name="TKey">Type of keys.</typeparam>
/// <typeparam name="TValue">Type of items, implementing the <see cref="IIncrementalKeyedCollectionItem{TKey}"/> interface.</typeparam>
[PublicAPI]
public partial class IncrementalKeyedCollection<TKey, TValue> : IOverridable, IReadOnlyCollection<TValue>, ICompileTimeSerializable
    where TKey : notnull
    where TValue : class, IIncrementalKeyedCollectionItem<TKey>
{
    public static IncrementalKeyedCollection<TKey, TValue> Empty { get; } = new( ImmutableDictionary<TKey, Item>.Empty );

    private readonly bool _clear;
    private ImmutableDictionary<TKey, Item> _dictionary;

    [NonCompileTimeSerialized]
    private int? _count;

    protected internal IncrementalKeyedCollection( ImmutableDictionary<TKey, Item> dictionary, bool clear = false )
    {
        this._dictionary = dictionary;
        this._clear = clear;
    }

    protected virtual IncrementalKeyedCollection<TKey, TValue> Create( ImmutableDictionary<TKey, Item> items, bool clear = false ) => new( items, clear );

    /// <summary>
    /// Creates a new <see cref="IncrementalKeyedCollection{TKey,TValue}"/> that represents the operation of removing any item both in the
    /// current collection and in any overridden collection.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> Clear() => this.Create( ImmutableDictionary<TKey, Item>.Empty, true );

    /// <summary>
    /// Creates a new <see cref="IncrementalKeyedCollection{TKey,TValue}"/> that represents the operation of adding an item to
    /// the collection, or to override with new value if this item already exists, additionally to any operation
    /// represented by the current object.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> AddOrOverride( TValue item )
    {
        var key = item.Key;

        TValue mergedItem;

        if ( this._dictionary.TryGetValue( key, out var oldItem ) && oldItem.IsEnabled )
        {
            mergedItem = oldItem.Value.OverrideWithSafe( item, default )!;
        }
        else
        {
            mergedItem = item;
        }

        return this.Create( this._dictionary.SetItem( key, new Item( mergedItem ) ) );
    }

    /// <summary>
    /// Creates a new <see cref="IncrementalKeyedCollection{TKey,TValue}"/> that represents the operation of adding items to
    /// the collection, or to override with new value if this item already exists, additionally to any operation
    /// represented by the current object.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> AddOrOverride( params TValue[] items ) => this.AddOrOverride( (IEnumerable<TValue>) items );

    /// <summary>
    /// Creates a new <see cref="IncrementalKeyedCollection{TKey,TValue}"/> that represents the operation of adding items to
    /// the collection, or to override with new value if this item already exists, additionally to any operation
    /// represented by the current object.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> AddOrOverride( IEnumerable<TValue> items )
    {
        var builder = this._dictionary.ToBuilder();

        foreach ( var item in items )
        {
            var key = item.Key;

            TValue mergedItem;

            if ( this._dictionary.TryGetValue( key, out var oldItem ) && oldItem.IsEnabled )
            {
                mergedItem = oldItem.Value.OverrideWithSafe( item, default )!;
            }
            else
            {
                mergedItem = item;
            }

            builder[item.Key] = new Item( mergedItem );
        }

        return this.Create( builder.ToImmutable(), this._clear );
    }

    /// <summary>
    /// Creates a new <see cref="IncrementalKeyedCollection{TKey,TValue}"/> that represents the option of removing an item
    /// from the current collection or from the overridden collection, additionally to other operations represented by the current object.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> Remove( TKey key ) => this.Create( this._dictionary.SetItem( key, new Item( default, false ) ) );

    /// <summary>
    /// Creates a new <see cref="IncrementalKeyedCollection{TKey,TValue}"/> that represents the option of removing items
    /// from the current collection or from the overridden collection, additionally to other operations represented by the current object.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> Remove( TKey[] keys ) => this.Remove( (IEnumerable<TKey>) keys );

    /// <summary>
    /// Creates a new <see cref="IncrementalKeyedCollection{TKey,TValue}"/> that represents the option of removing items
    /// from the current collection or from the overridden collection, additionally to other operations represented by the current object.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> Remove( IEnumerable<TKey> keys )
    {
        var builder = this._dictionary.ToBuilder();

        foreach ( var key in keys )
        {
            if ( !(builder.TryGetValue( key, out var item ) && !item.IsEnabled) )
            {
                builder[key] = new Item( default, false );
            }
        }

        return this.Create( builder.ToImmutable(), this._clear );
    }

    public IEnumerator<TValue> GetEnumerator() => this._dictionary.Values.Where( i => i.IsEnabled ).Select( i => i.Value! ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Gets the number of items in the current collection.
    /// </summary>
    public int Count => this._count ??= this._dictionary.Count( i => i.Value.IsEnabled );

    public bool IsEmpty => this.Count == 0;

    /// <summary>
    /// Overrides the current collection with another collection and returns the result.
    /// </summary>
    public IncrementalKeyedCollection<TKey, TValue> OverrideWith( IncrementalKeyedCollection<TKey, TValue> overridingOptions, in OverrideContext context )
    {
        var dictionary = overridingOptions._clear ? ImmutableDictionary<TKey, Item>.Empty : this._dictionary;

        foreach ( var item in overridingOptions._dictionary )
        {
            if ( item.Value.IsEnabled && dictionary.TryGetValue( item.Key, out var existingItem ) && existingItem.IsEnabled )
            {
                // If we replace an enabled value by another enabled value, we have to merge the items.
                var newValue = (TValue) existingItem.Value!.OverrideWith( item.Value.Value!, context );
                dictionary = dictionary.SetItem( item.Key, new Item( newValue ) );
            }
            else
            {
                // In all other cases, the new item wins.
                dictionary = dictionary.SetItem( item.Key, item.Value );
            }
        }

        return this.Create( dictionary );
    }

    object IOverridable.OverrideWith( object other, in OverrideContext context )
        => this.OverrideWith( (IncrementalKeyedCollection<TKey, TValue>) other, context );
}