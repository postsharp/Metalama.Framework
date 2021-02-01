namespace Caravela.Reactive
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