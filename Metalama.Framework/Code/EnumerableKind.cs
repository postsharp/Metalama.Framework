// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable InconsistentNaming

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Kinds of iterators.
    /// </summary>
    public enum EnumerableKind
    {
        /// <summary>
        /// None. The method is not a <c>yield</c> iterator.
        /// </summary>
        None,

        /// <summary>
        /// An iterator returning a generic <see cref="System.Collections.Generic.IEnumerable{T}" />.
        /// </summary>
        IEnumerable,

        /// <summary>
        /// An iterator returning a generic <see cref="System.Collections.Generic.IEnumerator{T}" />.
        /// </summary>
        IEnumerator,

        /// <summary>
        /// An iterator returning a non-generic <see cref="System.Collections.IEnumerable" />.
        /// </summary>
        UntypedIEnumerable,

        /// <summary>
        /// An iterator returning a non-generic <see cref="System.Collections.IEnumerator" />.
        /// </summary>
        UntypedIEnumerator,

        /// <summary>
        /// An iterator returning <c>System.Collections.Generic.IAsyncEnumerable</c>.
        /// </summary>
        IAsyncEnumerable,

        /// <summary>
        /// An iterator returning <c>System.Collections.Generic.IAsyncEnumerator</c>.
        /// </summary>
        IAsyncEnumerator
    }
}