// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Defines where a symbol or an expression can be used, i.e. in compile-time code, in run-time code, or both.
    /// </summary>
    internal enum SymbolDeclarationScope
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
        /// Using of the symbol is unknown yet. It can be used both at compile time or at run time.
        /// This is typically for parameters of lambda expressions. The node is evaluated as unknown if at least one children is unknown
        /// doesn't depends on other children.
        /// </summary>
        Unknown
    }
}