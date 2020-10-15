using System;
using System.ComponentModel;

namespace Caravela.Reactive
{
    public interface IReactiveSource<TValue,TObserver> : INotifyPropertyChanged, IReactiveObservable<TObserver>
        where TObserver : IReactiveObserver
    {
        ReactiveVersionedValue<TValue> VersionedValue { get; }

        TValue Value { get; }
    }
}