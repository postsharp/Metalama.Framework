// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Defines where a symbol or an expression can be used, i.e. in compile-time code, in run-time code, or both.
    /// </summary>
    internal enum TemplatingScope
    {
        /// <summary>
        /// The symbol can be used both at compile time or at run time.
        /// The node has not been classified as necessarily compile-time or run-time.
        /// This is typically the case for symbols of system libraries and
        /// aspects, or any declaration marked with <see cref="CompileTimeAttribute"/>.
        /// </summary>
        Both,

        /// <summary>
        /// The symbol can be only used at run time only. This is the case for any symbol that is
        /// not contained in a system library and that is not annotated with <see cref="CompileTimeOnlyAttribute"/> or <see cref="CompileTimeAttribute"/>.
        /// The node must be evaluated at run-time, but its children can be compile-time expressions.
        /// </summary>
        RunTimeOnly,

        /// <summary>
        /// The symbol can be used only at compile time. This is the case for the compile-time API of
        /// Caravela, which is marked by <see cref="CompileTimeOnlyAttribute"/>.
        /// The node including all children nodes must be evaluated at compile time.
        /// </summary>
        CompileTimeOnly,

        /// <summary>
        /// Unknown scope, for instance the scope of a lambda parameter that is not bound to a context.
        /// </summary>
        Unknown,

        /// <summary>
        /// A <see cref="CompileTimeOnly"/> member whose evaluated value is <see cref="RunTimeOnly"/>. The return
        /// value does not need to be of the <c>dynamic</c> itself. However, the member must be decorated with both
        /// <see cref="CompileTimeAttribute"/> (possibly inherited) and <see cref="RunTimeOnlyAttribute"/>.
        /// </summary>
        CompileTimeDynamic,

        /// <summary>
        /// A member of a dynamic receiver.
        /// </summary>
        Dynamic,
        Conflict
    }
}