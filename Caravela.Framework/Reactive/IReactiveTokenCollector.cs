namespace Caravela.Reactive
{
    internal interface IReactiveTokenCollector
    {
        void AddDependency(IReactiveObservable<IReactiveObserver> observable);
    }
}