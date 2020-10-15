using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Reactive
{
    public static class ReactiveSourceExtensions
    {
        public static IGroupBy<TKey, TItem> GroupBy<TKey, TItem>(
            this IReactiveCollection<TItem> source, Func<TItem, ReactiveCollectorToken, TKey> getKeyFunc,
            IEqualityComparer<TKey>? equalityComparer = default)
        {
            return new GroupByOperator<TKey, TItem>(source, getKeyFunc, equalityComparer);
        }

        public static IGroupBy<TKey, TItem> GroupBy<TKey, TItem>(
            this IReactiveCollection<TItem> source, Func<TItem, TKey> getKeyFunc,
            IEqualityComparer<TKey>? equalityComparer = default)
        {
            return new GroupByOperator<TKey, TItem>(source, (item, token) => getKeyFunc(item), equalityComparer);
        }


        public static IReactiveCollection<T> Union<T>(this IReactiveCollection<T> source, IReactiveCollection<T> second)
        {
            return new UnionOperator<T>(source, second);
        }

        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IReactiveCollection<TResult>> func)
        {
            return new SelectManyObservableOperator<TSource, TResult>(source, func);
        }

        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source, Func<TSource, IReactiveCollection<TResult>> func)
        {
            return new SelectManyObservableOperator<TSource, TResult>(source, (source1, token) => func(source1));
        }

        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, IImmutableList<TResult>> func)
        {
            return new SelectManyImmutableOperator<TSource, TResult>(source, func);
        }


        public static IReactiveCollection<TResult> SelectMany<TSource, TResult>(
            this IReactiveCollection<TSource> source, Func<TSource, IImmutableList<TResult>> func)
        {
            return new SelectManyImmutableOperator<TSource, TResult>(source, (source1, token) => func(source1));
        }


        public static IReactiveCollection<TResult> Expand<TResult>(
            this IReactiveCollection<IReactiveCollection<TResult>> source)
        {
            return new SelectManyObservableOperator<IReactiveCollection<TResult>, TResult>(source,
                (collection, token) => collection);
        }


        public static IReactiveCollection<T> SelectManyRecursive<T>(
            this IReactiveCollection<T> source,
            Func<T, ReactiveCollectorToken, IReactiveCollection<T>> getRecursionValueFunc)
            where T : class
        {
            return new SelectManyRecursiveOperator<T>(source, getRecursionValueFunc);
        }


        public static IReactiveCollection<T> SelectManyRecursive<T>(
            this IReactiveCollection<T> source, Func<T, IReactiveCollection<T>> getRecursionValueFunc)
            where T : class
        {
            return new SelectManyRecursiveOperator<T>(source, (arg1, token) => getRecursionValueFunc(arg1));
        }


        public static IReactiveCollection<T> SelectRecursive<T>(
            this IReactiveCollection<T> source, Func<T, T> getRecursionValueFunc, Func<T, bool>? stopPredicate = null)
            where T : class
        {
            throw new NotImplementedException();
        }


        public static IReactiveCollection<TSource> Where<TSource>(this IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, bool> func)
        {
            return new WhereOperator<TSource>(source, func);
        }

        public static IReactiveCollection<TSource> Where<TSource>(this IReactiveCollection<TSource> source,
            Func<TSource, bool> func)
        {
            return new WhereOperator<TSource>(source, (source1, token) => func(source1));
        }

        public static IDisposable ForEach<T>(this IReactiveCollection<T> source, Action<T>? addAction = null,
            Action<T>? removeAction = null)
        {
            return new ForEachOperator<T>(source, addAction, removeAction);
        }

        public static IReactiveCollection<TResult> Select<TSource, TResult>(this IReactiveCollection<TSource> source,
            Func<TSource, ReactiveCollectorToken, TResult> func)
        {
            return new SelectOperator<TSource, TResult>(source, func);
        }

        public static IReactiveCollection<TResult> Select<TSource, TResult>(this IReactiveCollection<TSource> source,
            Func<TSource, TResult> func)
        {
            return new SelectOperator<TSource, TResult>(source, (source1, token) => func(source1));
        }
        
        public static IReactiveCollection<T> Materialize<T>(this IReactiveCollection<T> source)
        {
            if (source.IsMaterialized)
            {
                return source;
            }
            else
            {
                return new MaterializeOperator<T>(source);
            }
        }


        public static IReactiveCollection<TResult> OfType<TSource, TResult>(this IReactiveCollection<TSource> source)
        {
            throw new NotImplementedException();
        }
    }
}