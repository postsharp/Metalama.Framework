// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Reactive
{
    /// <summary>
    /// Represents the <see cref="ReactiveSourceExtensions.GroupBy{TKey,TItem}"/> operator.
    /// </summary>
    /// <typeparam name="TKey">Type of the group key.</typeparam>
    /// <typeparam name="TItem">Type of group items.</typeparam>
    public interface IReactiveGroup<out TKey, out TItem> : IReactiveCollection<TItem>
    {
        /// <summary>
        /// Gets the group key.
        /// </summary>
        TKey Key { get; }
    }
}