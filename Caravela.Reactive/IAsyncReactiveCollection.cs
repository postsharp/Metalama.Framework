// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Caravela.Reactive
{
    /// <summary>
    /// A reactive <see cref="IEnumerable{T}"/> that supports asynchronous operators.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IAsyncReactiveCollection<T> : IAsyncReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}