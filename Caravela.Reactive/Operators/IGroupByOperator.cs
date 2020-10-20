using System.Collections.Generic;

namespace Caravela.Reactive.Operators
{
    internal interface IGroupByOperator<TKey, TElement> : IReactiveSource<IEnumerable<IReactiveGroup<TKey, TElement>>>
    {
        void EnsureSubscribedToSource();
    }
}
