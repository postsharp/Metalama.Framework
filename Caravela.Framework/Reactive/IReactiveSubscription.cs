#region

using System;

#endregion

namespace Caravela.Reactive
{
    public interface IReactiveSubscription : IDisposable
    {
        object Sender { get; }
        IReactiveObserver Observer { get; }
    }

    public interface IReactiveSubscription<out T> : IReactiveSubscription
        where T : IReactiveObserver
    {
        new T Observer { get; }
    }
}