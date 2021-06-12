// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Kinds of <see cref="IType"/>.
    /// </summary>
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
        GenericParameter,
        
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
        Struct

        // FunctionPointer
    }
}