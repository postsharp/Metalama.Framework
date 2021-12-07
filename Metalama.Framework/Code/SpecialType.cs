// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

// ReSharper disable InconsistentNaming

namespace Metalama.Framework.Code
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

        Byte,

        SByte,

        Int16,

        UInt16,

        Int32,

        UInt32,

        Int64,

        UInt64,

        String,

        Decimal,

        Single,

        Double,

        /// <summary>
        /// <c>System.Collections.IEnumerable</c>.
        /// </summary>
        IEnumerable,

        /// <summary>
        /// <c>System.Collections.IEnumerator</c>.
        /// </summary>
        IEnumerator,

        /// <summary>
        /// <c>System.Collections.Generic.IEnumerable</c>.
        /// </summary>
        IEnumerable_T,

        /// <summary>
        /// <c>System.Collections.Generic.IEnumerator</c>.
        /// </summary>
        IEnumerator_T,

        /// <summary>
        /// <c>System.Collections.Generic.List</c>.
        /// </summary>
        List_T,

        /// <summary>
        /// <c>System.Collections.Generic.IAsyncEnumerable</c>.
        /// </summary>
        IAsyncEnumerable_T,

        /// <summary>
        /// <c>System.Collections.Generic.IAsyncEnumerator</c>.
        /// </summary>
        IAsyncEnumerator_T,

        ValueTask,

        ValueTask_T,

        Task,

        Task_T,

        // Must be last.

        /// <summary>
        /// Number of items in this enumeration.
        /// </summary>
        Count
    }
}