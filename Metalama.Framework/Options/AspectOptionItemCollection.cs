// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Options;

public sealed partial class AspectOptionItemCollection<T> : IOverridable, IReadOnlyCollection<T>, ICompileTimeSerializable
    where T : IAspectOptionItem
{
    private readonly ImmutableDictionary<object, Item> _items;
    private readonly bool _clear;

    public AspectOptionItemCollection( params T[] items )
    {
        this._items = items.ToImmutableDictionary( i => i.GetKey(), i => new Item( i ) );
    }

    private AspectOptionItemCollection( ImmutableDictionary<object, Item> items, bool clear = false )
    {
        this._items = items;
        this._clear = clear;
    }

    public AspectOptionItemCollection<T> Clear() => new( this._items, true );

    public AspectOptionItemCollection<T> AddOrUpdate( T item )
    {
        var key = item.GetKey();

        return new AspectOptionItemCollection<T>( this._items.SetItem( key, new Item( item ) ) );
    }

    public AspectOptionItemCollection<T> Remove( T item ) => new( this._items.SetItem( item, new Item( item, false ) ) );

    public IEnumerator<T> GetEnumerator() => this._items.Values.Where( i => i.IsEnabled ).Select( i => i.Value! ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public int Count => this._items.Count( i => i.Value.IsEnabled );

    public AspectOptionItemCollection<T> OverrideWith( AspectOptionItemCollection<T> options, in AspectOptionsOverrideContext context )
    {
        var dictionary = options._clear ? ImmutableDictionary<object, Item>.Empty : this._items;

        foreach ( var item in options._items )
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

        return new AspectOptionItemCollection<T>( dictionary );
    }

    object IOverridable.OverrideWith( object options, in AspectOptionsOverrideContext context )
        => this.OverrideWith( (AspectOptionItemCollection<T>) options, context );
}