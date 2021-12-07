// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Reactive.Operators
{
    internal interface IGroupByOperator<TKey, TElement> : IReactiveSource<IEnumerable<IReactiveGroup<TKey, TElement>>>
    {
        void EnsureSubscribedToSource();
    }
}
