namespace Caravela.Reactive
{
    /// <summary>
    /// Represents the <see cref="ReactiveSourceExtensions.GroupBy{TKey,TItem}"/> operator.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface IReactiveGroup<out TKey, out TItem> : IReactiveCollection<TItem>
    {
        TKey Key { get; }
    }
}