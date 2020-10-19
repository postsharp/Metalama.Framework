using System;

namespace Caravela.Reactive
{
   

    /// <summary>
    /// Represents a subscription of an <see cref="IReactiveObserver"/> to an <see cref="IReactiveObservable{T}"/>
    /// </summary>
    public interface IReactiveSubscription : IDisposable
    {
        object Observable { get; }
        IReactiveObserver Observer { get; }
    }
    
    /// <summary>
    /// A strongly-typed specialization of <see cref="IReactiveSubscription"/>.
    /// </summary>
    /// <typeparam name="T">Type of observer.</typeparam>
    public interface IReactiveSubscription<out T> : IReactiveSubscription
        where T : IReactiveObserver
    {
        new T Observer { get; }
    }
}