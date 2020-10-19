#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Caravela.Reactive
{
    public static class ReactiveSourceExtensions
    {
        public static IReactiveGroupBy<TKey, TItem> GroupBy<TKey, TItem>(
            this IReactiveCollection<TItem> source, Func<TItem, TKey> getKeyFunc,
            IEqualityComparer<TKey> equalityComparer = default)
        {
            return new GroupByOperator<TKey, TItem>(source, getKeyFunc, equalityComparer);
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
            this IReactiveCollection<T> source, Func<T, T> getRecursionValueFunc, Func<T, bool> stopPredicate = null)
            where T : class
        {
            throw new NotImplementedException();
        }
        
     


        public static IReactiveCollection<TSource> Where<TSource>(this IReactiveCollection<TSource> source,
            Func<TSource, bool> func)
        {
            return new WhereOperator<TSource>(source, func);
        }

   
        public static IDisposable WriteLine<T>(this IReactiveCollection<T> source, string name = null)
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
        
        public static IReactiveSource<T,IReactiveObserver<T>> First<T>(
            this IReactiveCollection<T> source, Func<T, bool> func)
        {
            return new FirstOperator<T>(source, func, false);
        }
        
        public static IReactiveSource<T,IReactiveObserver<T>> FirstOrDefault<T>(
            this IReactiveCollection<T> source, Func<T, bool> func)
        {
            return new FirstOperator<T>(source, func, true);
        }
    }
}