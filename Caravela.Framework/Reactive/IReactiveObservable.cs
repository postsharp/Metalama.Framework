namespace Caravela.Reactive
{

    public interface IReactiveDebugging
    {
        bool HasPathToObserver(object observer);
    }

    public interface IReactiveObservable<in T> : IReactiveDebugging
        where T : IReactiveObserver
    {
        // This is the original object, which may be different to the helper object that implements the interface.
        object Object { get; }
        IReactiveSubscription AddObserver(T observer);
        bool RemoveObserver(IReactiveSubscription subscription);

       
    }
}