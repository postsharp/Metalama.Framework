using System.Collections.Generic;

namespace Caravela.Reactive
{
    internal interface IGroupByOperator<TKey, TElement> : IReactiveSource<IEnumerable<IReactiveGroup<TKey, TElement>>>
    {
        void EnsureSubscribedToSource();
    }
}
