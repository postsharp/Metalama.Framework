using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Reactive
{
    internal class FirstOperator<T> : ReactiveOperator<IEnumerable<T>,IReactiveCollectionObserver<T>,T,IReactiveObserver<T>>, IReactiveCollectionObserver<T>
    {
        private T _result;
        static readonly IEqualityComparer<T> _equalityComparer = EqualityComparerFactory.GetEqualityComparer<T>();
        private readonly Func<T, ReactiveCollectorToken, bool> _predicate;
        private readonly bool _orDefault;

        public FirstOperator(IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>> source, Func<T, ReactiveCollectorToken, bool> predicate, bool orDefault) : base(source)
        {
            this._predicate = predicate;
            this._orDefault = orDefault;
        }

        protected internal override IReactiveSubscription SubscribeToSource()
        {
            return this.Source.AddObserver(this);
        }

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            T newResult;
            newResult = this._orDefault 
                ? source.FirstOrDefault(arg => this._predicate(arg, this.CollectorToken))
                : source.First(arg => this._predicate(arg, this.CollectorToken));

            this._result = newResult;
            return true;
        }

        protected override T GetFunctionResult() => _result;


        void IReactiveCollectionObserver<T>.OnItemAdded(IReactiveSubscription subscription, T item, int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            var oldResult = _result;
            
            if (!_equalityComparer.Equals(oldResult, default) && this._predicate(item, this.CollectorToken))
            {
                using UpdateToken token = this.GetIncrementalUpdateToken();

                token.SignalChange();
                this._result = item;

                foreach (var observer in Observers)
                {
                    observer.Observer.OnValueChanged(observer.Subscription, oldResult, item, token.Version);
                }
            }
        }

        void IReactiveCollectionObserver<T>.OnItemRemoved(IReactiveSubscription subscription, T item, int newVersion)
        {
            if (!this.CanProcessIncrementalChange)
                return;
            
            var oldResult = _result;
            
            if (!_equalityComparer.Equals(oldResult, item) && this._predicate(item, this.CollectorToken))
            {
                this.OnBreakingChange();
            }
        }

        void IReactiveCollectionObserver<T>.OnItemReplaced(IReactiveSubscription subscription, T oldItem, T newItem, int newVersion)
        {
            
            if (!this.CanProcessIncrementalChange)
                return;
            
            var oldResult = _result;
            
            if (!_equalityComparer.Equals(oldResult, oldItem)
                && this._predicate(oldItem, this.CollectorToken))
            {
                this.OnBreakingChange();
            }
        }
    }
}