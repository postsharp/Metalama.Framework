#region

using System;
using System.Collections.Generic;

#endregion

namespace Caravela.Reactive
{
    /// <summary>
    /// A weakly-typed observer to a reactive object. Does not support incremental changes.
    /// </summary>
    public interface IReactiveObserver : IDisposable
    {
        /// <summary>
        ///     Signals that the previous value has become invalid and the new value has not yet been evaluated.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="isBreakingChange">
        ///     Specifies that the value cannot be reconstructed
        ///     by reacting to incremental events such as those of <see cref="IReactiveCollectionObserver{T}" />.
        /// </param>
        void OnValueInvalidated(IReactiveSubscription subscription, bool isBreakingChange);
    }

    /// <summary>
    /// A strongly-typed observer that supports notifications that the value has changed, but does not
    /// give more detail about the nature of the change.
    /// </summary>
    /// <typeparam name="T">Type of value.</typeparam>
    public interface IReactiveObserver<in T> : IReactiveObserver
    {
        /// <summary>
        ///     Signals that the value has changed and the new value is known.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="newVersion"></param>
        /// <param name="isBreakingChange">
        ///     Specifies that <paramref name="newValue" /> cannot be incrementally evaluated
        ///     by reacting to incremental  events such as those of <see cref="IReactiveCollectionObserver{T}" />.
        /// </param>
        void OnValueChanged(IReactiveSubscription subscription, T oldValue, T newValue, int newVersion,
            bool isBreakingChange = false);
    }

    /// <summary>
    /// An observer specialized for <see cref="IEnumerable{T}"/>. Supports notifications of changes
    /// in a collection: <see cref="OnItemAdded"/>, <see cref="OnItemRemoved"/> and <see cref="OnItemReplaced"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactiveCollectionObserver<in T> : IReactiveObserver<IEnumerable<T>>
    {
        void OnItemAdded(IReactiveSubscription subscription, T item, int newVersion);
        void OnItemRemoved(IReactiveSubscription subscription, T item, int newVersion);
        void OnItemReplaced(IReactiveSubscription subscription, T oldItem, T newItem, int newVersion);
    }
}