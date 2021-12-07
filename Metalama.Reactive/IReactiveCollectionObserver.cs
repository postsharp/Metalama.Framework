// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Reactive
{
    /// <summary>
    /// An observer specialized for <see cref="IEnumerable{T}"/>. Supports notifications of changes
    /// in a collection: <see cref="OnItemAdded"/>, <see cref="OnItemRemoved"/> and <see cref="OnItemReplaced"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactiveCollectionObserver<in T> : IReactiveObserver<IEnumerable<T>>
    {
        /// <summary>
        /// Signals that an item was added to the collection.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="item"></param>
        /// <param name="newVersion">Version of the source after the item was added.</param>
        void OnItemAdded( IReactiveSubscription subscription, T item, int newVersion );

        /// <summary>
        /// Signals that an item from the collection.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="item"></param>
        /// <param name="newVersion">Version of the source after the item was removed.</param>
        void OnItemRemoved( IReactiveSubscription subscription, T item, int newVersion );

        /// <summary>
        /// Signals that an item was replaced in the collection.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="oldItem"></param>
        /// <param name="newItem"></param>
        /// <param name="newVersion">Version of the source after the item was replaced.</param>
        void OnItemReplaced( IReactiveSubscription subscription, T oldItem, T newItem, int newVersion );
    }
}