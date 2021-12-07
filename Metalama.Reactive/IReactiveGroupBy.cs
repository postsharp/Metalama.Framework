// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Reactive
{
    /// <summary>
    /// Represents a group in a <see cref="ReactiveSourceExtensions.GroupBy{TKey,TItem}"/> operator.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface IReactiveGroupBy<TKey, out TItem> : IReactiveCollection<IReactiveGroup<TKey, TItem>>
    {
        /// <summary>
        /// Gets a specific group. This indexer always returns a non-null group so that it is possible
        /// to register observers.
        /// </summary>
        /// <param name="key">The group key.</param>
        IReactiveGroup<TKey, TItem> this[TKey key] { get; }
    }
}