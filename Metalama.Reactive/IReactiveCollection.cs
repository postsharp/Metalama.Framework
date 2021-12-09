// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;

namespace Metalama.Reactive
{
    /// <summary>
    /// A reactive <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    public interface IReactiveCollection<out T> : IReactiveSource<IEnumerable<T>, IReactiveCollectionObserver<T>>
    {
    }
}