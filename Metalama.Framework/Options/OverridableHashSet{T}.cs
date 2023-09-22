﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Options;

/// <summary>
/// An immutable hash set that implements the <see cref="IOverridable"/> semantic and can be easily used in the context of an <see cref="IHierarchicalOptions{T}"/>.
/// The class can represent the <see cref="Add(T)"/>, <see cref="Remove(T)"/> and <see cref="Clear"/> operations. To create an instance of this class,
/// use the <see cref="OverridableHashSet"/> factory class.
/// </summary>
/// <typeparam name="T">Type of items.</typeparam>
[PublicAPI]
public partial class OverridableHashSet<T> : IOverridable, IReadOnlyCollection<T>, ICompileTimeSerializable
    where T : notnull
{
    private readonly bool _clear;
    private ImmutableDictionary<T, bool> _dictionary;

    protected internal OverridableHashSet( ImmutableDictionary<T, bool> dictionary, bool clear = false )
    {
        this._dictionary = dictionary;
        this._clear = clear;
    }

    protected virtual OverridableHashSet<T> Create( ImmutableDictionary<T, bool> dictionary, bool clear = false ) => new( dictionary, clear );

    /// <summary>
    /// Creates a <see cref="OverridableHashSet{T}"/> that represents the operation of removing any item both in the
    /// current collection and in any overridden collection.
    /// </summary>
    public OverridableHashSet<T> Clear() => this.Create( ImmutableDictionary<T, bool>.Empty, true );

    /// <summary>
    /// Creates a new <see cref="OverridableHashSet{T}"/> that represents the operation of adding an item to
    /// the overridden collection, or to override with a new value if this item already exists, additionally to
    /// any operation represented by the current collection.
    /// </summary>
    public OverridableHashSet<T> Add( T item )
    {
        return this.Create( this._dictionary.SetItem( item, true ), this._clear );
    }

    /// <summary>
    /// Creates a new <see cref="OverridableHashSet{T}"/> that represents the operation of adding items to
    /// the overridden collection, or to override with a new value if these items already exist, additionally to
    /// any operation represented by the current collection.
    /// </summary>
    public OverridableHashSet<T> Add( T[] items ) => this.Add( (IEnumerable<T>) items );

    /// <summary>
    /// Creates a new <see cref="OverridableHashSet{T}"/> that represents the operation of adding items to
    /// the overridden collection, or to override with a new value if these items already exist, additionally to
    /// any operation represented by the current collection.
    /// </summary>
    public OverridableHashSet<T> Add( IEnumerable<T> items )
    {
        var builder = this._dictionary.ToBuilder();

        foreach ( var item in items )
        {
            builder[item] = true;
        }

        return this.Create( builder.ToImmutable(), this._clear );
    }

    /// <summary>
    /// Creates a new <see cref="OverridableHashSet{T}"/> that represents the option of removing an item
    /// from the overridden collection, additionally to any operation represented by the current object.
    /// </summary>
    public OverridableHashSet<T> Remove( T item ) => this.Create( this._dictionary.SetItem( item, false ), this._clear );

    /// <summary>
    /// Creates a new <see cref="OverridableHashSet{T}"/> that represents the option of removing a items
    /// from the overridden collection, additionally to any operation represented by the current object.
    /// </summary>
    public OverridableHashSet<T> Remove( T[] items ) => this.Remove( (IEnumerable<T>) items );

    /// <summary>
    /// Creates a new <see cref="OverridableHashSet{T}"/> that represents the option of removing a items
    /// from the overridden collection, additionally to any operation represented by the current object.
    /// </summary>
    public OverridableHashSet<T> Remove( IEnumerable<T> items )
    {
        var builder = this._dictionary.ToBuilder();

        foreach ( var item in items )
        {
            builder[item] = false;
        }

        return this.Create( builder.ToImmutable(), this._clear );
    }

    public IEnumerator<T> GetEnumerator() => this._dictionary.Where( x => x.Value ).Select( x => x.Key ).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Gets the number of items in the current collection.
    /// </summary>
    public int Count => this._dictionary.Count( x => x.Value );

    /// <summary>
    /// Overrides the current collection with another collection and returns the result.
    /// </summary>
    public OverridableHashSet<T> OverrideWith( OverridableHashSet<T> other, in OverrideContext context )
    {
        if ( other._clear )
        {
            return other;
        }
        else
        {
            var items = this._dictionary;

            foreach ( var pair in other._dictionary )
            {
                items = items.SetItem( pair.Key, pair.Value );
            }

            return this.Create( items );
        }
    }

    object IOverridable.OverrideWith( object overridingObject, in OverrideContext context )
        => this.OverrideWith( (OverridableHashSet<T>) overridingObject, context );
}