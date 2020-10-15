namespace Caravela.Reactive
{
    public interface IReactiveSource<out TValue, in TObserver> : IReactiveObservable<TObserver>
        where TObserver : IReactiveObserver
    {
        TValue GetValue(in ReactiveCollectorToken collectorToken);
        IReactiveVersionedValue<TValue> GetVersionedValue(in ReactiveCollectorToken collectorToken);
        
        bool IsMaterialized { get; }
    }

    public interface IReactiveVersionedValue<out TValue>
    {
        int Version { get; }
        TValue Value { get; }
    }
}