// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Kinds of <see cref="IType"/>.
    /// </summary>
    [CompileTime]
    public enum TypeKind
    {
        /// <summary>
        /// Array.
        /// </summary>
        Array,

        /// <summary>
        /// Class.
        /// </summary>
        Class,

        RecordClass,

        /// <summary>
        /// Delegate.
        /// </summary>
        Delegate,

        /// <summary>
        /// <c>dynamic</c>.
        /// </summary>
        Dynamic,

        /// <summary>
        /// Enum.
        /// </summary>
        Enum,

        /// <summary>
        /// Generic parameter.
        /// </summary>
        TypeParameter,

        /// <summary>
        /// Interface.
        /// </summary>
        Interface,

        /// <summary>
        /// Unmanaged pointer.
        /// </summary>
        Pointer,

        /// <summary>
        /// Struct.
        /// </summary>
        Struct,

        RecordStruct

        // FunctionPointer
    }
}