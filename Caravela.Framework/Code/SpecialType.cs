// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

// ReSharper disable InconsistentNaming

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Special types, such as <see cref="Void"/>.
    /// </summary>
    [CompileTimeOnly]
    public enum SpecialType
    {
        /// <summary>
        /// Not a special type.
        /// </summary>
        None,

        /// <summary>
        /// <c>void</c>.
        /// </summary>
        Void,

        /// <summary>
        /// <c>object</c>.
        /// </summary>
        Object,

        IEnumerable,

        IEnumerator,

        IEnumerable_T,

        IEnumerator_T,

        List,

        IAsyncEnumerable,

        IAsyncEnumerator,

        // Must be last.
        Count
    }
}