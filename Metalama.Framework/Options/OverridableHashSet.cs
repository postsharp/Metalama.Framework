// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Options;

#pragma warning disable SA1642

/// <summary>
/// Factory methods for the <see cref="OverridableHashSet{T}"/> generic class.
/// </summary>
[CompileTime]
public static class OverridableHashSet
{
    /// <summary>
    /// Creates  new <see cref="OverridableHashSet{T}"/> that represents the absence of any operation.
    /// </summary>
    public static OverridableHashSet<T> Empty<T>()
        where T : notnull
        => new( ImmutableDictionary<T, bool>.Empty );

    /// <summary>
    /// Creates  new <see cref="OverridableHashSet{T}"/> that represents the operation clearing
    /// the overridden collection of all items.
    /// </summary>
    public static OverridableHashSet<T> Clear<T>()
        where T : notnull
        => new( ImmutableDictionary<T, bool>.Empty, true );

    /// <summary>
    /// Creates  new <see cref="OverridableHashSet{T}"/> that represents the operation of adding an item to
    /// the overridden collection, or to override with a new value if these items already exist.
    /// </summary>
    public static OverridableHashSet<T> Add<T>( T item )
        where T : notnull
        => new(
            ImmutableDictionary.Create<T, bool>()
                .Add( item, true ) );

    /// <summary>
    /// Creates  new <see cref="OverridableHashSet{T}"/> that represents the operation of adding items to
    /// the overridden collection, or to override with a new value if these items already exist.
    /// </summary>
    public static OverridableHashSet<T> Add<T>( params T[] items )
        where T : notnull
        => new( items.ToImmutableDictionary( i => i, i => true ) );

    /// <summary>
    /// Creates  new <see cref="OverridableHashSet{T}"/> that represents the operation of adding items to
    /// the overridden collection, or to override with a new value if these items already exist.
    /// </summary>
    public static OverridableHashSet<T> Add<T>( IEnumerable<T> items )
        where T : notnull
        => new( items.ToImmutableDictionary( i => i, i => true ) );

    /// <summary>
    /// Creates a <see cref="OverridableHashSet{T}"/> that represents the option of removing an item
    /// from the overridden collection.
    /// </summary>
    public static OverridableHashSet<T> Remove<T>( T item )
        where T : notnull
        => new(
            ImmutableDictionary.Create<T, bool>()
                .Add( item, false ) );

    /// <summary>
    /// Creates a <see cref="OverridableHashSet{T}"/> that represents the option of removing items
    /// from the overridden collection.
    /// </summary>
    public static OverridableHashSet<T> Remove<T>( params T[] items )
        where T : notnull
        => new( items.ToImmutableDictionary( i => i, i => false ) );

    /// <summary>
    /// Creates a <see cref="OverridableHashSet{T}"/> that represents the option of removing items
    /// from the overridden collection.
    /// </summary>
    public static OverridableHashSet<T> Remove<T>( IEnumerable<T> items )
        where T : notnull
        => new( items.ToImmutableDictionary( i => i, i => true ) );
}