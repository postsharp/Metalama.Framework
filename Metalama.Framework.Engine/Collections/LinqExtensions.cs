// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace System.Linq;

public static class LinqExtensions
{
    [Obsolete( "Use SelectEnumerable, SelectArray, SelectImmutableArray or SelectCollection." )]
    public static IEnumerable<TOut> Select<TIn, TOut>( this IReadOnlyCollection<TIn> list, Func<TIn, TOut> func ) => SelectEnumerable( list, func );

    public static IEnumerable<TOut> SelectEnumerable<TIn, TOut>( this IReadOnlyCollection<TIn> list, Func<TIn, TOut> func )
    {
        foreach ( var item in list )
        {
            yield return func( item );
        }
    }

    public static IEnumerable<TOut> SelectEnumerable<TIn, TOut>( this IReadOnlyList<TIn> list, Func<TIn, TOut> func )
    {
        foreach ( var item in list )
        {
            yield return func( item );
        }
    }

    public static IEnumerable<TOut> SelectEnumerable<TIn, TOut>( this ImmutableArray<TIn> list, Func<TIn, TOut> func )
    {
        foreach ( var item in list )
        {
            yield return func( item );
        }
    }

    public static TOut[] SelectArray<TIn, TOut>( this IReadOnlyList<TIn> list, Func<TIn, TOut> func )
    {
        var result = new TOut[list.Count];

        for ( var i = 0; i < result.Length; i++ )
        {
            result[i] = func( list[i] );
        }

        return result;
    }

    public static TOut[] SelectArray<TIn, TOut>( this IReadOnlyCollection<TIn> list, Func<TIn, TOut> func )
    {
        var result = new TOut[list.Count];

        var i = 0;

        foreach ( var item in list )
        {
            result[i] = func( item );
            i++;
        }

        return result;
    }

    public static TOut[] SelectArray<TIn, TOut>( this ImmutableArray<TIn> list, Func<TIn, TOut> func )
    {
        var result = new TOut[list.Length];

        for ( var i = 0; i < result.Length; i++ )
        {
            result[i] = func( list[i] );
        }

        return result;
    }

    public static List<TOut> SelectList<TIn, TOut>( this IReadOnlyCollection<TIn> list, Func<TIn, TOut> func )
    {
        var result = new List<TOut>( list.Count + 4 );

        foreach ( var item in list )
        {
            result.Add( func( item ) );
        }

        return result;
    }

    public static List<TOut> SelectList<TIn, TOut>( this IReadOnlyList<TIn> list, Func<TIn, TOut> func )
    {
        var result = new List<TOut>( list.Count + 4 );

        foreach ( var item in list )
        {
            result.Add( func( item ) );
        }

        return result;
    }

    public static List<TOut> SelectList<TIn, TOut>( this ImmutableArray<TIn> list, Func<TIn, TOut> func )
    {
        var result = new List<TOut>( list.Length + 4 );

        foreach ( var item in list )
        {
            result.Add( func( item ) );
        }

        return result;
    }

    public static ImmutableArray<TOut> SelectImmutableArray<TIn, TOut>( this ImmutableArray<TIn> list, Func<TIn, TOut> func )
    {
        var result = ImmutableArray.CreateBuilder<TOut>( list.Length );

        foreach ( var item in list )
        {
            result.Add( func( item ) );
        }

        return result.MoveToImmutable();
    }

    public static ImmutableArray<TOut> SelectImmutableArray<TIn, TOut>( this IReadOnlyList<TIn> list, Func<TIn, TOut> func )
    {
        var result = ImmutableArray.CreateBuilder<TOut>( list.Count );

        foreach ( var t in list )
        {
            result.Add( func( t ) );
        }

        return result.MoveToImmutable();
    }

    public static ImmutableArray<TOut> SelectImmutableArray<TIn, TOut>( this IReadOnlyCollection<TIn> list, Func<TIn, TOut> func )
    {
        var result = ImmutableArray.CreateBuilder<TOut>( list.Count );

        foreach ( var t in list )
        {
            result.Add( func( t ) );
        }

        return result.MoveToImmutable();
    }

    [Obsolete( "This method is redundant." )]
    public static T[] ToArray<T>( this T[] array ) => array;

    [Obsolete( "Use ToReadOnlyList or ToMutableList" )]
    public static List<T> ToList<T>( this IReadOnlyList<T> list ) => ToMutableList( list );

    public static List<T> ToMutableList<T>( this IReadOnlyList<T> list )
    {
        var result = new List<T>( list.Count );
        result.AddRange( list );

        return result;
    }

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> to an <see cref="IReadOnlyList{T}"/>, but calls <see cref="Enumerable.ToList{TSource}"/>
    /// only if needed.
    /// </summary>
    public static IReadOnlyList<T> ToReadOnlyList<T>( this IEnumerable<T> collection ) => collection as IReadOnlyList<T> ?? collection.ToList();

    [Obsolete( "This method is redundant." )]
    public static IReadOnlyList<T> ToReadOnlyList<T>( this IReadOnlyList<T> list ) => list;

    public static IReadOnlyCollection<T> AsReadOnly<T>( this ICollection<T> collection )
        => collection as IReadOnlyCollection<T> ?? new ReadOnlyCollectionWrapper<T>( collection );

    /// <summary>
    /// Converts an <see cref="IEnumerable"/> to an <see cref="IReadOnlyList{T}"/>, but calls <see cref="Enumerable.ToList{TSource}"/>
    /// only if needed.
    /// </summary>
    public static IReadOnlyList<object> ToReadOnlyList( this IEnumerable collection )
        => collection as IReadOnlyList<object> ?? new List<object>( collection.Cast<object>() );

    [Obsolete( "This method call is redundant." )]
    public static ImmutableArray<T> ToImmutableArray<T>( this ImmutableArray<T> list ) => list;

    public static ImmutableArray<T> ToImmutableArray<T>( this IReadOnlyList<T> list )
    {
        var builder = ImmutableArray.CreateBuilder<T>( list.Count );

        foreach ( var item in list )
        {
            builder.Add( item );
        }

        return builder.MoveToImmutable();
    }

    public static ImmutableArray<T> ToImmutableArray<T>( this IReadOnlyCollection<T> list )
    {
        var builder = ImmutableArray.CreateBuilder<T>( list.Count );

        foreach ( var item in list )
        {
            builder.Add( item );
        }

        return builder.MoveToImmutable();
    }

    public static ImmutableArray<T> ToImmutableArray<T>( this T[] list ) => ImmutableArray.Create( list );

    public static IEnumerable<T> Concat<T>( this IEnumerable<T> list, T item )
    {
        foreach ( var i in list )
        {
            yield return i;
        }

        yield return item;
    }

    public static IReadOnlyList<T> ConcatList<T>( this IReadOnlyList<T>? a, IReadOnlyList<T>? b )
    {
        if ( b == null || b.Count == 0 )
        {
            return a ?? Array.Empty<T>();
        }
        else if ( a == null || a.Count == 0 )
        {
            return b;
        }
        else
        {
            var result = new T[a.Count + b.Count];

            for ( var i = 0; i < a.Count; i++ )
            {
                result[i] = a[i];
            }

            for ( var i = 0; i < b.Count; i++ )
            {
                result[i + a.Count] = b[i];
            }

            return result;
        }
    }

    public static IReadOnlyList<T> ConcatList<T>( this IReadOnlyList<T> a, IReadOnlyList<T> b, IReadOnlyList<T> c )
    {
        var result = new T[a.Count + b.Count + c.Count];

        for ( var i = 0; i < a.Count; i++ )
        {
            result[i] = a[i];
        }

        for ( var i = 0; i < b.Count; i++ )
        {
            result[i + a.Count] = b[i];
        }

        for ( var i = 0; i < c.Count; i++ )
        {
            result[i + a.Count + b.Count] = c[i];
        }

        return result;
    }

    public static ImmutableArray<T> ConcatImmutableArray<T>( this IReadOnlyList<T> a, IReadOnlyList<T> b )
    {
        var result = ImmutableArray.CreateBuilder<T>( a.Count + b.Count );

        foreach ( var item in a )
        {
            result.Add( item );
        }

        foreach ( var item in b )
        {
            result.Add( item );
        }

        return result.MoveToImmutable();
    }

    public static ImmutableArray<T> ConcatImmutableArray<T>( this IReadOnlyList<T> a, IReadOnlyList<T> b, IReadOnlyList<T> c )
    {
        var result = ImmutableArray.CreateBuilder<T>( a.Count + b.Count + c.Count );

        foreach ( var item in a )
        {
            result.Add( item );
        }

        foreach ( var item in b )
        {
            result.Add( item );
        }

        foreach ( var item in c )
        {
            result.Add( item );
        }

        return result.MoveToImmutable();
    }

    [Obsolete( "Use ToOrderedList" )]
    public static List<T> ToList<T>( this IOrderedEnumerable<T> enumerable ) => ((IEnumerable<T>) enumerable).ToList();

    public static List<TItem> ToOrderedList<TItem, TKey>(
        this IEnumerable<TItem> enumerable,
        Func<TItem, TKey> orderBy,
        IComparer<TKey>? comparer = null,
        bool descending = false )
    {
        var list = enumerable.ToList();

        if ( list.Count < 2 )
        {
            return list;
        }

        var multiplier = descending ? -1 : 1;
        comparer ??= Comparer<TKey>.Default;
        list.Sort( ( x, y ) => multiplier * comparer.Compare( orderBy( x ), orderBy( y ) ) );

        return list;
    }

    public static List<TItem> ToOrderedList<TItem, TKey>(
        this IReadOnlyCollection<TItem> enumerable,
        Func<TItem, TKey> orderBy,
        IComparer<TKey>? comparer = null,
        bool descending = false )
    {
        var list = new List<TItem>( enumerable.Count );
        list.AddRange( enumerable );

        if ( list.Count < 2 )
        {
            return list;
        }

        var multiplier = descending ? -1 : 1;
        comparer ??= Comparer<TKey>.Default;

        list.Sort( ( x, y ) => multiplier * comparer.Compare( orderBy( x ), orderBy( y ) ) );

        return list;
    }

    public static bool Any<T>( this IReadOnlyCollection<T> collection ) => collection.Count > 0;

    public static T? FirstOrDefault<T>( this IReadOnlyList<T> list ) => list.Count == 0 ? default : list[0];

    public static T First<T>( this IReadOnlyList<T> list ) => list.Count > 0 ? list[0] : throw new InvalidOperationException();
}