namespace Caravela.Reactive.Implementation
{
    partial class ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver>
    {
        /// <summary>
        /// Implementation of <see cref="IReactiveObservable{T}"/> used for <see cref="ReactiveObserverToken"/>.
        /// It is not possible to use the main class for this interface implementation because of potential conflicts.
        /// </summary>
        private class DependencyObservable : IReactiveObservable<IReactiveObserver>
        {
            private readonly ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> _parent;

            public DependencyObservable(ReactiveOperator<TSource, TSourceObserver, TResult, TResultObserver> parent)
            {
                this._parent = parent;
            }

            int IReactiveObservable<IReactiveObserver>.Version => this._parent.Version;

            public object Object => this._parent;

            IReactiveSubscription IReactiveObservable<IReactiveObserver>.AddObserver(IReactiveObserver observer)
            {
                return this._parent._observers.AddObserver(observer);
            }

            bool IReactiveObservable<IReactiveObserver>.RemoveObserver(IReactiveSubscription subscription)
            {
                return this._parent._observers.RemoveObserver(subscription);
            }
        }
    }
}