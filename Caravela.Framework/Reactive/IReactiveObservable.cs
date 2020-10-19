using System;

namespace Caravela.Reactive
{
    /// <summary>
    /// An observable is something to which observers (<see cref="IReactiveObserver"/>) can subscribe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactiveObservable<in T> where T : IReactiveObserver
    {
        /// <summary>
        /// Gets the current version of the observable object.
        /// </summary>
        int Version { get; }
        
        // This is the original object, which may be different to the helper object that implements the interface.
        object Object { get; }
        
        /// <summary>
        /// Adds an observer.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>An object that allows unsubscription either using <see cref="IDisposable.Dispose"/>
        ///    or <see cref="RemoveObserver"/>.
        /// </returns>
        IReactiveSubscription? AddObserver(T observer);
        
        bool RemoveObserver(IReactiveSubscription subscription);
       
    }
}