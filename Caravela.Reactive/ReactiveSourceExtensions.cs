#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Reactive.Operators;

#endregion

namespace Caravela.Reactive
{
    public static class ReactiveSourceExtensions
    {
        public static IReactiveGroupBy<TKey, TItem> GroupBy<TKey, TItem>(
            this IReactiveCollection<TItem> source, Func<TItem, TKey> getKeyFunc,
            IEqualityComparer<TKey>? equalityComparer = default)
        {
            return new GroupByOperator<TItem, TKey, TItem>(source, getKeyFunc, item => item, equalityComparer);
        }

        public static IReactiveGroupBy<TKey, TElement> GroupBy<TSource, TKey, TElement>(
            this IReactiveCollection<TSource> source, Func<TSource, TKey> getKeyFunc, Func<TSource, TElement> getElementFunc,
            IEqualityComparer<TKey>? equalityComparer = default)
        {
            return new GroupByOperator<TSource, TKey, TElement>(source, getKeyFunc, getElementFunc, equalityComparer);
        }

        public static IReactiveCollection<T> Union<T>(this IReactiveCollection<T> source, IReactiveCollection<T> second)
        {
            return new UnionOperator<T>(source, second);
        }

        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, IReactiveCollection<TResult>> func)
        {
            return new SelectManyObservableOperator<TSource, TResult>(source, func);
        }


        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, IImmutableList<TResult>> func)
        {
            return new SelectManyImmutableOperator<TSource, TResult>(source, func);
        }


        public static IReactiveCollection<TResult> SelectMany<TSource, TCollection, TResult>(
            this IReactiveCollection<TSource> source, Func<TSource, IReactiveCollection<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return new SelectManyObservableOperator<TSource, TCollection, TResult>(
                source, collectionSelector, resultSelector);
        }

        public static IReactiveCollection<TResult> SelectMany<TSource, TCollection, TResult>(
            this IReactiveCollection<TSource> source, Func<TSource, IImmutableList<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return new SelectManyImmutableOperator<TSource, TCollection, TResult>(
                source, collectionSelector, resultSelector);
        }

        public static IReactiveCollection<TResult> Expand<TResult>(
            this IReactiveCollection<IReactiveCollection<TResult>> source)
        {
            return new SelectManyObservableOperator<IReactiveCollection<TResult>, TResult>(source,
                collection => collection);
        }


        public static IReactiveCollection<T> SelectManyRecursive<T>(
            this IReactiveCollection<T> source,
            Func<T,  IReactiveCollection<T>> getRecursionValueFunc)
            where T : class
        {
            return new SelectManyRecursiveOperator<T>(source, getRecursionValueFunc);
        }


    
        public static IReactiveCollection<T> SelectRecursive<T>(
            this IReactiveCollection<T> source, Func<T, T> getRecursionValueFunc, Func<T, bool>? stopPredicate = null)
            where T : class
        {
            throw new NotImplementedException();
        }
        
     


        public static IReactiveCollection<TSource> Where<TSource>(this IReactiveCollection<TSource> source,
            Func<TSource, bool> func)
        {
            return new WhereOperator<TSource>(source, func);
        }

   
        public static IDisposable WriteLine<T>(this IReactiveCollection<T> source, string? name = null)
        {
            return new WriteLineOperator<T>(source, name);
        }

        public static IReactiveCollection<TResult> Select<TSource, TResult>(this IReactiveCollection<TSource> source,
            Func<TSource, TResult> func)
        {
            return new SelectOperator<TSource, TResult>(source, func);
        }

        public static IReactiveCollection<TResult> SelectCached<TSource, TResult>(this IReactiveCollection<TSource> source,
            Func<TSource,  TResult> func) where TSource : class where TResult : class
        {
            return new SelectCachedOperator<TSource, TResult>(source, func);
        }


        public static IReactiveCollection<T> Materialize<T>(this IReactiveCollection<T> source)
        {
            if (source.IsMaterialized)
            {
                return source;
            }
            else
            {
                return new ToListOperator<T>(source);
            }
        }


        public static IReactiveCollection<TResult> OfType<TSource, TResult>(this IReactiveCollection<TSource> source)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Chooses <em>some</em> value from <paramref name="source"/> that matches <paramref name="func"/>.
        /// If there are multiple matching values, there are no guarantees about which one is chosen.
        /// </summary>
        public static IReactiveSource<T,IReactiveObserver<T>> Some<T>(
            this IReactiveCollection<T> source, Func<T, bool>? func = null)
        {
            func ??= _ => true;

            return new SomeOperator<T>(source, func, false);
        }
        
        public static IReactiveSource<T,IReactiveObserver<T>> SomeOrDefault<T>(
            this IReactiveCollection<T> source, Func<T, bool>? func = null)
        {
            func ??= _ => true;

            return new SomeOperator<T>(source, func, true);
        }
    }
}