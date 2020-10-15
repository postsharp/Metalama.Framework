namespace Caravela.Reactive
{
    public interface IGroupBy<TKey, TItem> : IReactiveCollection<Group<TKey, TItem>>
    {
        Group<TKey, TItem> this[TKey key] { get; }
    }
}