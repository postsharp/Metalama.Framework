namespace Caravela.Reactive
{
    public interface IReactiveObservable<in T>
        where T : IReactiveObserver
    {
        IReactiveSubscription AddObserver(T observer);
        bool RemoveObserver(IReactiveSubscription subscription);
    }
}