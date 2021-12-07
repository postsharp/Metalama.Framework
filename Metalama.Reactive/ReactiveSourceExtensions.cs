// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Metalama.Reactive.Operators;
using Metalama.Reactive.Sources;

namespace Metalama.Reactive
{
    /// <summary>
    /// Extension methods for <see cref="IReactiveCollection{T}"/>.
    /// </summary>
    public static class ReactiveSourceExtensions
    {
        /// <summary>
        /// Groups the input source by a given key and returns a collection of groups (each group being reactive),
        /// where the values of the group are the source items themselves.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="getKeyFunc">A function that returns the grouping key of an item.</param>
        /// <param name="equalityComparer">An equality comparer (optional).</param>
        /// <typeparam name="TKey">Type of keys.</typeparam>
        /// <typeparam name="TItem">Type of items.</typeparam>
        /// <returns></returns>
        public static IReactiveGroupBy<TKey, TItem> GroupBy<TKey, TItem>(
            this IReactiveCollection<TItem> source,
            Func<TItem, TKey> getKeyFunc,
            IEqualityComparer<TKey>? equalityComparer = default )
        {
            return new GroupByOperator<TItem, TKey, TItem>( source, getKeyFunc, item => item, equalityComparer );
        }

        /// <summary>
        /// Groups the input source by a given key and returns a collection of groups (each group being reactive),
        /// where the values of the group are mapped from the source items by a delegate.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="getKeyFunc">A function that returns the grouping key of an item.</param>
        /// <param name="getElementFunc">A function that returns the output group items.</param>
        /// <param name="equalityComparer"></param>
        /// <typeparam name="TSource">Type of source items.</typeparam>
        /// <typeparam name="TKey">Type of group keys.</typeparam>
        /// <typeparam name="TResult">Type of output items.</typeparam>
        /// <returns></returns>
        public static IReactiveGroupBy<TKey, TResult> GroupBy<TSource, TKey, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, TKey> getKeyFunc,
            Func<TSource, TResult> getElementFunc,
            IEqualityComparer<TKey>? equalityComparer = default )
        {
            return new GroupByOperator<TSource, TKey, TResult>( source, getKeyFunc, getElementFunc, equalityComparer );
        }

        /// <summary>
        /// Groups the input by a given key by enumerating the input collection in a given order and by creating a group
        /// every time the key value has changed. Therefore, the output set of groups may contain several groups
        /// of the same key. In this overload, the items of the output groups are identical to the source items.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceComparer">The comparer used to order the items.</param>
        /// <param name="getKeyFunc">A function that returns the grouping key of an item.</param>
        /// <typeparam name="TKey">Type of group keys.</typeparam>
        /// <typeparam name="TItem">Type of items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<IReactiveGroup<TKey, TItem>> OrderedGroupBy<TKey, TItem>(
            this IReactiveCollection<TItem> source,
            IComparer<TItem> sourceComparer,
            Func<TItem, TKey> getKeyFunc )
        {
            return new OrderedGroupByOperator<TItem, TKey, TItem>( source, sourceComparer, getKeyFunc, item => item, null );
        }

        /// <summary>
        /// Groups the input by a given key by enumerating the input collection in a given order and by creating a group
        /// every time the key value has changed. Therefore, the output set of groups may contain several groups
        /// of the same key. In this overload, the items of the output groups can be of a different type than the source items.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="sourceComparer">The comparer used to order the items.</param>
        /// <param name="getKeyFunc">A function that returns the grouping key of an item.</param>
        /// <param name="getElementFunc">A function that returns the output item from the source item.</param>
        /// <typeparam name="TKey">Type of group keys.</typeparam>
        /// <typeparam name="TSource">Type of source items.</typeparam>
        /// <typeparam name="TResult">Type of result items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<IReactiveGroup<TKey, TResult>> OrderedGroupBy<TSource, TKey, TResult>(
            this IReactiveCollection<TSource> source,
            IComparer<TSource> sourceComparer,
            Func<TSource, TKey> getKeyFunc,
            Func<TSource, TResult> getElementFunc )
        {
            return new OrderedGroupByOperator<TSource, TKey, TResult>( source, sourceComparer, getKeyFunc, getElementFunc, null );
        }

        /// <summary>
        /// Appends two sources, without guarantees of ordering and without deduplication.
        /// </summary>
        /// <param name="source">The first source.</param>
        /// <param name="second">The second source.</param>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<T> Union<T>( this IReactiveCollection<T> source, IReactiveCollection<T> second )
        {
            return new UnionOperator<T>( source, second );
        }

        /// <summary>
        /// Projects each element of an observation collection to an observable collection and flattens the resulting sequences into one observable collection.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="func">A function that returns a collection of output items given an input item.</param>
        /// <typeparam name="TSource">Type of source items.</typeparam>
        /// <typeparam name="TResult">Type of output items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, IReactiveCollection<TResult>> func )
        {
            return new SelectManyObservableOperator<TSource, TResult>( source, func );
        }

        /// <summary>
        /// Projects each element of an observation collection to an immutable collection and flattens the resulting sequences into one observable collection.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="func">A function that returns a collection of output items given an input item.</param>
        /// <typeparam name="TSource">Type of source items.</typeparam>
        /// <typeparam name="TResult">Type of output items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, IImmutableList<TResult>> func )
        {
            return new SelectManyImmutableOperator<TSource, TResult>( source, func );
        }

        /// <summary>
        /// Projects each element of an observable collection to an observable collection, flattens the resulting collection into one observable collection,
        /// and invokes a result selector function on each element therein, resulting in the output observable collection. This overload is typically used
        /// by language-integrated queries with a non-trivial <c>select</c> clause.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="collectionSelector">A function that returns a collection of intermediate items given an input item.</param>
        /// <param name="resultSelector">A function that returns the output item from the source item and the intermediate collection.</param>
        /// <typeparam name="TSource">Type of source items.</typeparam>
        /// <typeparam name="TResult">Type of output items.</typeparam>
        /// <typeparam name="TCollection">Type of intermediate items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<TResult> SelectMany<TSource, TCollection, TResult>(
            this IReactiveCollection<TSource> source, Func<TSource, IReactiveCollection<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector )
        {
            return new SelectManyObservableOperator<TSource, TCollection, TResult>(
                source, collectionSelector, resultSelector );
        }

        /// <summary>
        /// Projects each element of an observable collection to an immutable collection, flattens the resulting collection into one observable collection,
        /// and invokes a result selector function on each element therein, resulting in the output observable collection. This overload is typically used
        /// by language-integrated queries with a non-trivial <c>select</c> clause.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="collectionSelector">A function that returns a collection of intermediate items given an input item.</param>
        /// <param name="resultSelector">A function that returns the output item from the source item and the intermediate collection.</param>
        /// <typeparam name="TSource">Type of source items.</typeparam>
        /// <typeparam name="TResult">Type of output items.</typeparam>
        /// <typeparam name="TCollection">Type of intermediate items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<TResult> SelectMany<TSource, TCollection, TResult>(
            this IReactiveCollection<TSource> source, Func<TSource, IImmutableList<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector )
        {
            return new SelectManyImmutableOperator<TSource, TCollection, TResult>(
                source, collectionSelector, resultSelector );
        }

        /*
        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
         this IReactiveSource<TSource, IReactiveObserver<TSource>> source, Func<TSource, IImmutableList<TResult>> collectionSelector )
        {
            return new SelectManyFromScalarOperator<TSource, TResult>( source, collectionSelector );
        }

        public static IReactiveCollection<TResult> Expand<TResult>(
            this IReactiveCollection<IReactiveCollection<TResult>> source)
        {
            return new SelectManyObservableOperator<IReactiveCollection<TResult>, TResult>(source,
                collection => collection);
        }
        */

        /// <summary>
        /// Recursively selects of all descendants in a tree.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="getChildrenFunc"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IReactiveCollection<T> SelectDescendants<T>(
            this IReactiveCollection<T> source,
            Func<T, IReactiveCollection<T>> getChildrenFunc )
            where T : class
        {
            return new SelectDescendantsOperator<T>( source, getChildrenFunc );
        }

        /// <summary>
        /// Filters an observable collection based on a predicate.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="predicate">A function that returns <c>true</c> if the input item must be included in the output collection.</param>
        /// <typeparam name="TSource">Type of items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<TSource> Where<TSource>(
            this IReactiveCollection<TSource> source,
            Func<TSource, bool> predicate )
        {
            return new WhereOperator<TSource>( source, predicate );
        }

        /// <summary>
        /// Writes operations on an observable collection to the <see cref="Console"/>. Useful for debugging.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDisposable WriteLine<T>( this IReactiveCollection<T> source, string? name = null )
        {
            return new WriteLineOperator<T>( source, name );
        }

        /// <summary>
        /// Projects each element of an observable collection into a new form.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="func">A function mapping the input to the output.</param>
        /// <typeparam name="TSource">Type of input items.</typeparam>
        /// <typeparam name="TResult">Type of output items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<TResult> Select<TSource, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, TResult> func )
        {
            return new SelectOperator<TSource, TResult>( source, func );
        }

        /*
        public static IReactiveCollection<TResult> SelectCached<TSource, TResult>(this IReactiveCollection<TSource> source,
            Func<TSource,  TResult> func) where TSource : class where TResult : class
        {
            return new SelectCachedOperator<TSource, TResult>(source, func);
        }
        */

        /// <summary>
        /// Returns a materialized collection, i.e. a collection that is "cached" and that can be enumerated
        /// several times without enumerating the source. If the source is already materialize, this method does
        /// nothing. To determine if a source is materialized, use <see cref="IReactiveSource.IsMaterialized"/>.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<T> Materialize<T>( this IReactiveCollection<T> source )
        {
            if ( source.IsMaterialized )
            {
                return source;
            }
            else
            {
                return new ToListOperator<T>( source );
            }
        }

        /// <summary>
        /// Filters an input observable collection and returns only items of a given type.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <typeparam name="TSource">Type of source items.</typeparam>
        /// <typeparam name="TResult">Type of output items.</typeparam>
        /// <returns></returns>
        public static IReactiveCollection<TResult> OfType<TSource, TResult>( this IReactiveCollection<TSource> source )
            where TResult : class
            => source.Select( s => s as TResult ).Where( s => s != null )!;

        /// <summary>
        /// Chooses from an observable collection an arbitrary value that matches an optional given predicate.
        /// If there are multiple matching values, there are no guarantees about which one is chosen. This method
        /// throws <see cref="InvalidOperationException"/> if no item in the source matches the predicate.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="predicate">A function that returns <c>true</c> if the item can be selected, or <c>null</c> if any
        /// item can be selected.</param>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <returns></returns>
        public static IReactiveSource<T, IReactiveObserver<T>> Some<T>(
            this IReactiveCollection<T> source, Func<T, bool>? predicate = null )
        {
            predicate ??= _ => true;

            return new SomeOperator<T>( source, predicate, false );
        }

        /// <summary>
        /// Chooses from an observable collection an arbitrary value that matches an optional given predicate.
        /// If there are multiple matching values, there are no guarantees about which one is chosen. This method
        /// returns the default value of the output type if no item in the source matches the predicate.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="predicate">A function that returns <c>true</c> if the item can be selected, or <c>null</c> if any
        /// item can be selected.</param>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <returns></returns>
        public static IReactiveSource<T, IReactiveObserver<T>> SomeOrDefault<T>(
            this IReactiveCollection<T> source, Func<T, bool>? predicate = null )
        {
            predicate ??= _ => true;

            return new SomeOperator<T>( source, predicate, true );
        }

        /// <summary>
        /// Wraps a source <see cref="IImmutableList{T}"/> into an <see cref="IReactiveCollection{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of items.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IReactiveCollection<T> ToReactive<T>( this IImmutableList<T> source ) => new ImmutableReactiveCollection<T>( source );

        /// <summary>
        /// Wraps a source <see cref="IImmutableSet{T}"/> into an <see cref="IReactiveCollection{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IReactiveCollection<T> ToReactive<T>( this IImmutableSet<T> source ) => new ImmutableReactiveCollection<T>( source );

        /// <summary>
        /// Assumes that a source <see cref="IEnumerable{T}"/> is immutable and wraps it into an <see cref="IReactiveCollection{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IReactiveCollection<T> ToImmutableReactive<T>( this IEnumerable<T> source ) => new ImmutableReactiveCollection<T>( source );
    }
}