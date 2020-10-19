namespace Caravela.Reactive
{
    public interface IReactiveTokenCollector
    {
        void AddDependency(IReactiveObservable<IReactiveObserver> source, int version);
    }
}