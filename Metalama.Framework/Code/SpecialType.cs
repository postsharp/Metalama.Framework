// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;

// ReSharper disable InconsistentNaming

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Special types, such as <see cref="Void"/>.
    /// </summary>
    [CompileTime]
    public enum SpecialType
    {
        /// <summary>
        /// Not a special type.
        /// </summary>
        None,

        /// <summary>
        /// <see cref="System.Void" />.
        /// </summary>
        Void,

        /// <summary>
        /// <see cref="object" />.
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
        
        Boolean,

        /// <summary>
        /// <see cref="System.Collections.IEnumerable" />.
        /// </summary>
        IEnumerable,

        /// <summary>
        /// <see cref="System.Collections.IEnumerator" />.
        /// </summary>
        IEnumerator,

        /// <summary>
        /// <see cref="System.Collections.Generic.IEnumerable{T}" />.
        /// </summary>
        IEnumerable_T,

        /// <summary>
        /// <see cref="System.Collections.Generic.IEnumerator{T}" />.
        /// </summary>
        IEnumerator_T,

        /// <summary>
        /// <see cref="System.Collections.Generic.List{T}" />.
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