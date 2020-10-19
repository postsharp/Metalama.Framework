using System;
using System.Threading;

namespace Caravela.Reactive
{
    /// <summary>
    /// A token passed to <see cref="IReactiveSource{T}.GetValue"/> or <see cref="IReactiveSource{T}.GetVersionedValue"/>.
    /// Exposes the most primitive (weakly-typed and non-incremental) way to collect dependencies to observables.
    /// As an end-user you should generally not use <see cref="ReactiveObserverToken"/> directly. You should pass
    /// the default instance instead. The default instance resolves to <see cref="CurrentCollector"/>, which can
    /// be modified using the <see cref="WithDefaultCollector"/> method. 
    /// </summary>
    public readonly struct ReactiveObserverToken
    {
        private static readonly NullCollector _nullCollector = new NullCollector();

        private static readonly ThreadLocal<IReactiveTokenCollector> _defaultCollector =
            new ThreadLocal<IReactiveTokenCollector>(() => _nullCollector);

        internal static IReactiveTokenCollector CurrentCollector
        {
            get => _defaultCollector.Value;
        }

        /// <summary>
        /// Modified the value of the <see cref="CurrentCollector"/> property for the current scope.
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static WithDefaultObserverToken WithDefaultCollector(IReactiveTokenCollector collector)
        {
            var previousCollector = CurrentCollector;
            _defaultCollector.Value = collector;
            return new WithDefaultObserverToken(previousCollector);
        }
        
        

        private readonly IReactiveTokenCollector _collector;

        internal IReactiveTokenCollector Collector => this._collector ?? CurrentCollector;

        internal ReactiveObserverToken(IReactiveTokenCollector collector)
        {
            this._collector = collector;
        }

        /// <summary>
        /// Wraps a function, to be used in a <see cref="ReactiveOperator{TSource,TSourceObserver,TResult,TResultObserver}"/>,
        /// so that it sets the <see cref="CurrentCollector"/> while executing.
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        internal static Func<TIn, ReactiveObserverToken, TOut> WrapWithDefaultToken<TIn, TOut>(
            Func<TIn, TOut> func)
            => delegate(TIn value, ReactiveObserverToken collectorToken)
            {
                using var tk = WithDefaultCollector(collectorToken._collector);
                return func(value);
            };

        /// <summary>
        /// Wraps a function, to be used in a <see cref="ReactiveOperator{TSource,TSourceObserver,TResult,TResultObserver}"/>,
        /// so that it sets the <see cref="CurrentCollector"/> while executing.
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        internal static Func<T1, T2, ReactiveObserverToken, TOut> WrapWithDefaultToken<T1, T2, TOut>(
            Func<T1, T2, TOut> func)
            => delegate (T1 x1, T2 x2, ReactiveObserverToken collectorToken)
            {
                using var tk = WithDefaultCollector(collectorToken._collector);
                return func(x1, x2);
            };

        /// <summary>
        /// Represents the absence of collector. We must use a different value than <c>null</c> so that
        /// <c>null</c>, the default value, can represent the fallback to <see cref="CurrentCollector"/>.
        /// </summary>
        private class NullCollector : IReactiveTokenCollector
        {
            void IReactiveTokenCollector.AddDependency(IReactiveObservable<IReactiveObserver> source, int version)
            {
            }
        }
        
        /// <summary>
        /// An opaque return value for <see cref="ReactiveObserverToken.WithDefaultCollector"/>, to be disposed
        /// at the end of the scope.
        /// </summary>
        public struct WithDefaultObserverToken : IDisposable
        {
            private  IReactiveTokenCollector _previousCollector;

            internal WithDefaultObserverToken(IReactiveTokenCollector previousCollector)
            {
                this._previousCollector = previousCollector;
            }

            public void Dispose()
            {
                if (this._previousCollector != null)
                {
                    _defaultCollector.Value = this._previousCollector;
                    this._previousCollector = null!;

                }
            }
        }
    }

 
}