// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Options;

#pragma warning disable SA1642

/// <summary>
/// An immutable collection that implements the <see cref="IOverridable"/> semantic and can be easily used in the context of an <see cref="IHierarchicalOptions{T}"/>.
/// The class can represent the <see cref="AddOrOverride"/>, <see cref="Remove"/> and <see cref="Clear"/> operations.
/// </summary>
/// <typeparam name="T">Type of items, implementing the <see cref="IHierarchicalOptionItem"/> interface.</typeparam>
[PublicAPI]
public sealed partial class HierarchicalOptionItemCollection<T> : IOverridable, IReadOnlyCollection<T>, ICompileTimeSerializable
    where T : class, IHierarchicalOptionItem
{
    private readonly bool _clear;
    private ImmutableDictionary<object, Item> _items;

    /// <summary>
    /// Creates a new, empty instance of the <see cref="HierarchicalOptionItemCollection{T}"/>. This instance does not represent any operation.
    /// It is typically followed by a call to <see cref="Clear"/> or <see cref="Remove"/>. 
    /// </summary>
    public HierarchicalOptionItemCollection()
    {
        this._items = ImmutableDictionary<object, Item>.Empty;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="HierarchicalOptionItemCollection{T}"/> that represents the operation of adding items to the collection.   
    /// </summary>
    public HierarchicalOptionItemCollection( params T[] items )
    {
        this._items = items.ToImmutableDictionary( i => i.GetKey(), i => new Item( i ) );
    }

    private HierarchicalOptionItemCollection( ImmutableDictionary<object, Item> items, bool clear = false )
    {
        this._items = items;
        this._clear = clear;
    }

    /// <summary>
    /// Creates a <see cref="HierarchicalOptionItemCollection{T}"/> that represents the operation of removing any item both in the
    /// current collection and in any overridden collection.
    /// </summary>
    public HierarchicalOptionItemCollection<T> Clear() => new( ImmutableDictionary<object, Item>.Empty, true );

    /// <summary>
    /// Creates a <see cref="HierarchicalOptionItemCollection{T}"/> that represents the operation of adding an item to
    /// the overridden collection, or to override with new value if this item already exists.
    /// </summary>
    public HierarchicalOptionItemCollection<T> AddOrOverride( T item )
    {
        var key = item.GetKey();

        T mergedItem;

        if ( this._items.TryGetValue( key, out var oldItem ) && oldItem.IsEnabled )
        {
            mergedItem = oldItem.Value.OverrideWithSafe( item, default )!;
        }
        else
        {
            mergedItem = item;
        }

        return new HierarchicalOptionItemCollection<T>( this._items.SetItem( key, new Item( mergedItem ) ) );
    }

    /// <summary>
    /// Creates a <see cref="HierarchicalOptionItemCollection{T}"/> that represents the option of removing an item
    /// from the overridden collection.
    /// </summary>
    public HierarchicalOptionItemCollection<T> Remove( T item ) => new( this._items.SetItem( item, new Item( item, false ) ) );

    public IEnumerator<T> GetEnumerator() => this._items.Values.Where( i => i.IsEnabled ).Select( i => i.Value! ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Gets the number of items in the current collection.
    /// </summary>
    public int Count => this._items.Count( i => i.Value.IsEnabled );

    /// <summary>
    /// Overrides the current collection with another collection and returns the result.
    /// </summary>
    public HierarchicalOptionItemCollection<T> OverrideWith( HierarchicalOptionItemCollection<T> overridingOptions, in OverrideContext context )
    {
        var dictionary = overridingOptions._clear ? ImmutableDictionary<object, Item>.Empty : this._items;

        foreach ( var item in overridingOptions._items )
        {
            if ( item.Value.IsEnabled && dictionary.TryGetValue( item.Key, out var existingItem ) && existingItem.IsEnabled )
            {
                // If we replace an enabled value by another enabled value, we have to merge the items.
                var newValue = (T) existingItem.Value!.OverrideWith( item.Value.Value!, context );
                dictionary = dictionary.SetItem( item.Key, new Item( newValue ) );
            }
            else
            {
                // In all other cases, the new item wins.
                dictionary = dictionary.SetItem( item.Key, item.Value );
            }
        }

        return new HierarchicalOptionItemCollection<T>( dictionary );
    }

    object IOverridable.OverrideWith( object overridingObject, in OverrideContext context )
        => this.OverrideWith( (HierarchicalOptionItemCollection<T>) overridingObject, context );
}