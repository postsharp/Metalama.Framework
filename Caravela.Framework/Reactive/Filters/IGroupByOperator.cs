namespace Caravela.Reactive
{
    internal interface IGroupByOperator : IReactiveSource
    {
        void EnsureSubscribedToSource();
    }
}
