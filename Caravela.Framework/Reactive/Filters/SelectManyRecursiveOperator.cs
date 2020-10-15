using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Reactive
{
    internal class SelectManyRecursiveOperator<T> : ReactiveCollectionOperator<T, T>
        where T : class
    {
        private static readonly Func<T, bool> _defaultContinuePredicate = t => t != null;
        private readonly Func<T, bool> _continuePredicate;
        private readonly Func<T, ReactiveCollectorToken, IReactiveCollection<T>> _getRecursionValueFunc;
        private ImmutableDictionary<T, int> _result;

        private ImmutableDictionary<IReactiveCollection<T>, (IReactiveSubscription subscription, int count)>
            _subscriptions =
                ImmutableDictionary<IReactiveCollection<T>, (IReactiveSubscription subscription, int count)>.Empty;

        public SelectManyRecursiveOperator(IReactiveCollection<T> source,
            Func<T, ReactiveCollectorToken, IReactiveCollection<T>> getRecursionValueFunc,
            Func<T, bool> stopPredicate) : base(source)
        {
            _getRecursionValueFunc = getRecursionValueFunc;
            _continuePredicate = stopPredicate ?? _defaultContinuePredicate;
        }

        protected override bool EvaluateFunction(IEnumerable<T> source)
        {
            var builder = ImmutableDictionary.CreateBuilder<T, int>();

            void Iterate(T item)
            {
                builder.TryGetValue(item, out var count);
                builder[item] = count + 1;

                var recursiveSource = _getRecursionValueFunc(item, CollectorToken);

                if (Follow(recursiveSource))
                    foreach (var recursiveItem in recursiveSource.GetValue(CollectorToken))
                        Iterate(recursiveItem);
            }

            foreach (var item in source) Iterate(item);

            _result = builder.ToImmutable();
            return true;
        }

        private bool Follow(IReactiveCollection<T> source)
        {
            if (!_subscriptions.TryGetValue(source, out var existing))
            {
                _subscriptions = _subscriptions.Add(source, (source.AddObserver(this), 1));
                return true;
            }
            else
            {
                _subscriptions = _subscriptions.SetItem(source, (existing.subscription, existing.count + 1));
                return false;
            }
        }

        private bool Unfollow(IReactiveCollection<T> source)
        {
            if (!_subscriptions.TryGetValue(source, out var existing))
                return false;

            if (existing.count == 1)
            {
                _subscriptions = _subscriptions.Remove(source);
                return true;
            }
            else
            {
                _subscriptions = _subscriptions.SetItem(source, (existing.subscription, existing.count - 1));
                return false;
            }
        }

        protected override IEnumerable<T> GetFunctionResult()
        {
            return _result.Keys;
        }

        private void AddItem(T item, ref ImmutableDictionary<T, int> newResult, UpdateToken updateToken)
        {
            void Iterate(T item, ref ImmutableDictionary<T, int> newResult)
            {
                if (!newResult.TryGetValue(item, out var count))
                {
                    updateToken.SignalChange();
                    foreach (var subscription in Observers)
                        subscription.Observer.OnItemAdded(subscription, item, updateToken.Version);
                }

                newResult = newResult.SetItem(item, count + 1);

                var recursiveSource = _getRecursionValueFunc(item, CollectorToken);

                if (Follow(recursiveSource))
                    foreach (var recursiveItem in recursiveSource.GetValue(CollectorToken))
                        Iterate(recursiveItem, ref newResult);
            }

            Iterate(item, ref newResult);
        }

        private void RemoveItem(T item, ref ImmutableDictionary<T, int> newResult, UpdateToken updateToken)
        {
            void Iterate(T item, ref ImmutableDictionary<T, int> newResult)
            {
                if (!newResult.TryGetValue(item, out var count)) return;

                if (count == 1)
                {
                    updateToken.SignalChange();

                    foreach (var subscription in Observers)
                        subscription.Observer.OnItemRemoved(subscription, item, updateToken.Version);

                    newResult = newResult.Remove(item);
                }
                else
                {
                    newResult = newResult.SetItem(item, count - 1);
                }

                var recursiveSource = _getRecursionValueFunc(item, CollectorToken);

                if (Unfollow(recursiveSource))
                    foreach (var recursiveItem in recursiveSource.GetValue(CollectorToken))
                        Iterate(recursiveItem, ref newResult);
            }

            Iterate(item, ref newResult);
        }

        protected override void OnSourceItemAdded(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            var newResult = _result;

            AddItem(item, ref newResult, updateToken);

            _result = newResult;
        }


        protected override void OnSourceItemRemoved(IReactiveSubscription sourceSubscription, T item,
            in UpdateToken updateToken)
        {
            var newResult = _result;

            RemoveItem(item, ref newResult, updateToken);

            _result = newResult;
        }

        protected override void OnSourceItemReplaced(IReactiveSubscription sourceSubscription, T oldItem, T newItem,
            in UpdateToken updateToken)
        {
            var newResult = _result;

            RemoveItem(oldItem, ref newResult, updateToken);
            AddItem(newItem, ref newResult, updateToken);

            _result = newResult;
        }
    }
}