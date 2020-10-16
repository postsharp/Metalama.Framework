namespace Caravela.Reactive
{
    public interface IReactiveSource<out TValue, in TObserver> : IReactiveObservable<TObserver>
        where TObserver : IReactiveObserver
    {
        bool IsMaterialized { get; }
        TValue GetValue(in ReactiveCollectorToken collectorToken);
        IReactiveVersionedValue<TValue> GetVersionedValue(in ReactiveCollectorToken collectorToken);
    }

    public interface IReactiveVersionedValue<out TValue>
    {
        int Version { get; }
        TValue Value { get; }
    }
}