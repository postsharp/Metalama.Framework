#region

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

#endregion

namespace Caravela.Reactive.Operators
{
    internal class SelectManyRecursiveOperator<T> : ReactiveCollectionOperator<T, T>
        where T : class
    {
        private readonly Func<T, ReactiveObserverToken, IReactiveCollection<T>> _getRecursionValueFunc;
        private ImmutableDictionary<T, int> _result = null!;

        private ImmutableDictionary<IReactiveCollection<T>, (IReactiveSubscription? subscription, int count)>
            _subscriptions =
                ImmutableDictionary<IReactiveCollection<T>, (IReactiveSubscription? subscription, int count)>.Empty;

        public SelectManyRecursiveOperator(IReactiveCollection<T> source,
            Func<T, IReactiveCollection<T>> getRecursionValueFunc) : base(source)
        {
            this._getRecursionValueFunc = ReactiveObserverToken.WrapWithDefaultToken(getRecursionValueFunc);
        }

        public override bool IsMaterialized => true;

        protected override IEnumerable<T> EvaluateFunction(IEnumerable<T> source)
        {
            var builder = ImmutableDictionary.CreateBuilder<T, int>();

            void Iterate(T item)
            {
                builder.TryGetValue(item, out int count );
                builder[item] = count + 1;

                var recursiveSource = this._getRecursionValueFunc(item, this.ObserverToken);

                if (this.Follow(recursiveSource))
                {
                    foreach (var recursiveItem in recursiveSource.GetValue(this.ObserverToken))
                    {
                        Iterate(recursiveItem);
                    }
                }
            }

            foreach (var item in source)
            {
                Iterate(item);
            }

            this._result = builder.ToImmutable();
            return this._result.Keys;
        }

        private bool Follow(IReactiveCollection<T> source)
        {
            if (!this._subscriptions.TryGetValue(source, out var existing))
            {
                this._subscriptions = this._subscriptions.Add(source, (source.AddObserver(this), 1));
                return true;
            }
            else
            {
                this._subscriptions = this._subscriptions.SetItem(source, (existing.subscription, existing.count + 1));
                return false;
            }
        }

        private bool Unfollow(IReactiveCollection<T> source)
        {
            if (!this._subscriptions.TryGetValue(source, out var existing))
            {
                return false;
            }

            if (existing.count == 1)
            {
                this._subscriptions = this._subscriptions.Remove(source);
                return true;
            }
            else
            {
                this._subscriptions = this._subscriptions.SetItem(source, (existing.subscription, existing.count - 1));
                return false;
            }
        }

      
        private void AddItem(T item, ref ImmutableDictionary<T, int> newResult, IncrementalUpdateToken updateToken)
        {
            void Iterate(T item, ref ImmutableDictionary<T, int> newResult)
            {
                if (!newResult.TryGetValue(item, out int count ))
                {
                    updateToken.SignalChange();
                    foreach (var subscription in this.Observers)
                    {
                        subscription.Observer.OnItemAdded(subscription.Subscription, item, updateToken.NextVersion);
                    }
                }

                newResult = newResult.SetItem(item, count + 1);

                var recursiveSource = this._getRecursionValueFunc(item, this.ObserverToken);

                if (this.Follow(recursiveSource))
                {
                    foreach (var recursiveItem in recursiveSource.GetValue(this.ObserverToken))
                    {
                        Iterate(recursiveItem, ref newResult);
                    }
                }
            }

            Iterate(item, ref newResult);
        }

        private void RemoveItem(T item, ref ImmutableDictionary<T, int> newResult, IncrementalUpdateToken updateToken)
        {
            void Iterate(T item, ref ImmutableDictionary<T, int> newResult)
            {
                if (!newResult.TryGetValue(item, out int count ))
                {
                    return;
                }

                if (count == 1)
                {
                    updateToken.SignalChange();

                    foreach (var subscription in this.Observers)
                    {
                        subscription.Observer.OnItemRemoved(subscription.Subscription, item, updateToken.NextVersion);
                    }

                    newResult = newResult.Remove(item);
                }
                else
                {
                    newResult = newResult.SetItem(item, count - 1);
                }

                var recursiveSource = this._getRecursionValueFunc(item, this.ObserverToken);

                if (this.Unfollow(recursiveSource))
                {
                    foreach (var recursiveItem in recursiveSource.GetValue(this.ObserverToken))
                    {
                        Iterate(recursiveItem, ref newResult);
                    }
                }
            }

            Iterate(item, ref newResult);
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in IncrementalUpdateToken updateToken)
        {
            var newResult = this._result;

            this.AddItem(item, ref newResult, updateToken);

            this._result = newResult;
            
            updateToken.SetNewValue(newResult.Keys);
        }


        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in IncrementalUpdateToken updateToken)
        {
            var newResult = this._result;

            this.RemoveItem(item, ref newResult, updateToken);

            this._result = newResult;
            
            updateToken.SetNewValue(newResult.Keys);
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in IncrementalUpdateToken updateToken)
        {
            var newResult = this._result;

            this.RemoveItem(oldItem, ref newResult, updateToken);
            this.AddItem(newItem, ref newResult, updateToken);

            this._result = newResult;
            
            updateToken.SetNewValue(newResult.Keys);
        }
    }
}