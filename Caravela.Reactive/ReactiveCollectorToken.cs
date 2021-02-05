using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Caravela.Reactive.Implementation;

namespace Caravela.Reactive
{
    /// <summary>
    /// A token passed to <see cref="IReactiveSource{T}.GetValue"/> or <see cref="IReactiveSource{T}.GetVersionedValue"/>.
    /// Exposes the most primitive (weakly-typed and non-incremental) way to collect dependencies to observables.
    /// As an end-user you should generally not use <see cref="ReactiveCollectorToken"/> directly. You should pass
    /// the default instance instead. The default instance resolves to <see cref="CurrentCollector"/>, which can
    /// be modified using the <see cref="WithDefaultCollector"/> method.
    /// </summary>
    public readonly struct ReactiveCollectorToken
    {
        private static readonly NullCollector _nullCollector = new NullCollector();

        private static readonly ThreadLocal<IReactiveCollector> _defaultCollector =
            new ThreadLocal<IReactiveCollector>( () => _nullCollector );

        private static IReactiveCollector? CurrentCollector => _defaultCollector.Value;

        /// <summary>
        /// Modified the value of the <see cref="CurrentCollector"/> property for the current scope.
        /// </summary>
        /// <param name="collector"></param>
        /// <returns></returns>
        public static WithDefaultObserverToken WithDefaultCollector( IReactiveCollector? collector )
        {
            var previousCollector = CurrentCollector;
            _defaultCollector.Value = collector!;
            return new WithDefaultObserverToken( previousCollector );
        }

        private readonly IReactiveCollector? _collector;

        internal IReactiveCollector? Collector => this._collector ?? CurrentCollector;

        internal ReactiveCollectorToken( IReactiveCollector collector )
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
        internal static Func<TIn, ReactiveCollectorToken, TOut> WrapWithDefaultToken<TIn, TOut>( Func<TIn, TOut> func )
            => ( value, collectorToken ) =>
            {
                Debug.Assert( collectorToken._collector != null, "_collector should not be null" );

                using var tk = WithDefaultCollector( collectorToken._collector );
                var result = func( value );
                if ( result is IHasReactiveSideValues hasReactiveSideValues )
                {
                    collectorToken._collector!.AddSideValues( hasReactiveSideValues.SideValues );
                }

                return result;
            };

        internal static Func<TIn, ReactiveCollectorToken, CancellationToken, ValueTask<TOut>> WrapWithDefaultToken<TIn, TOut>(
          Func<TIn, CancellationToken, ValueTask<TOut>> func )
        {
            return async ( value, collectorToken, cancellationToken ) =>
            {
                Debug.Assert( collectorToken._collector != null, "_collector should not be null" );

                using var tk = WithDefaultCollector( collectorToken._collector );
                var result = await func( value, cancellationToken );
                if ( result is IHasReactiveSideValues hasReactiveSideValues )
                {
                    collectorToken._collector!.AddSideValues( hasReactiveSideValues.SideValues );
                }

                return result;
            };
        }

        /// <summary>
        /// Wraps a function, to be used in a <see cref="ReactiveOperator{TSource,TSourceObserver,TResult,TResultObserver}"/>,
        /// so that it sets the <see cref="CurrentCollector"/> while executing.
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        internal static Func<T1, T2, ReactiveCollectorToken, TOut> WrapWithDefaultToken<T1, T2, TOut>(
            Func<T1, T2, TOut> func )
            => delegate ( T1 x1, T2 x2, ReactiveCollectorToken collectorToken )
            {
                using var tk = WithDefaultCollector( collectorToken._collector );
                return func( x1, x2 );
            };

        /// <summary>
        /// Represents the absence of collector. We must use a different value than <c>null</c> so that
        /// <c>null</c>, the default value, can represent the fallback to <see cref="CurrentCollector"/>.
        /// </summary>
        private class NullCollector : IReactiveCollector
        {
            void IReactiveCollector.AddSideValue( IReactiveSideValue value )
            {
            }

            void IReactiveCollector.AddDependency( IReactiveObservable<IReactiveObserver> source, int version )
            {
            }

            void IReactiveCollector.AddSideValues( ReactiveSideValues values )
            {
            }
        }

        /// <summary>
        /// An opaque return value for <see cref="ReactiveCollectorToken.WithDefaultCollector"/>, to be disposed
        /// at the end of the scope.
        /// </summary>
        public struct WithDefaultObserverToken : IDisposable
        {
            private IReactiveCollector? _previousCollector;

            internal WithDefaultObserverToken( IReactiveCollector? previousCollector )
            {
                this._previousCollector = previousCollector;
            }

            public void Dispose()
            {
                if ( this._previousCollector != null )
                {
                    _defaultCollector.Value = this._previousCollector;
                    this._previousCollector = null!;
                }
            }
        }
    }
}