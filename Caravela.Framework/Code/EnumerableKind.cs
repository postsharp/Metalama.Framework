// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable InconsistentNaming

namespace Caravela.Framework.Code
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
        /// An iterator returning <c>System.Collections.Generic.IEnumerable</c>.
        /// </summary>
        IEnumerable,

        /// <summary>
        /// An iterator returning <c>System.Collections.Generic.IEnumerator</c>.
        /// </summary>
        IEnumerator,

        /// <summary>
        /// An iterator returning <c>System.Collections.IEnumerable</c>.
        /// </summary>
        UntypedIEnumerable,

        /// <summary>
        /// An iterator returning <c>System.Collections.IEnumerator</c>.
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