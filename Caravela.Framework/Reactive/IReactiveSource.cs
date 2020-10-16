namespace Caravela.Reactive
{
    public interface IReactiveSource 
    {
        bool IsMaterialized { get; }
        bool IsImmutable { get; }
        
        int Version { get; }
    }
    
    public interface IReactiveSource<out TValue, in TObserver> : IReactiveSource, IReactiveObservable<TObserver>
        where TObserver : IReactiveObserver
    {
      
        TValue GetValue(in ReactiveCollectorToken collectorToken);
        IReactiveVersionedValue<TValue> GetVersionedValue(in ReactiveCollectorToken collectorToken);
    }

    public interface IReactiveVersionedValue<out TValue>
    {
        int Version { get; }
        TValue Value { get; }
    }
}