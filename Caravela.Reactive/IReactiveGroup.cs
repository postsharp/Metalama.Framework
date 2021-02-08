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