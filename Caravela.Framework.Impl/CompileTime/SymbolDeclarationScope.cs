// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.CompileTime
{
    internal enum SymbolDeclarationScope
    {
        /// <summary>
        /// The symbol can be used both at compile time or at run time.
        /// The node has not been classified as necessarily compile-time or run-time.
        /// </summary>
        Default,

        /// <summary>
        /// The symbol can be only used at run time only.
        /// The node must be evaluated at run-time, but its children can be compile-time expressions.
        /// </summary>
        RunTimeOnly,

        /// <summary>
        /// The symbol can be used only at compile time.
        /// The node including all children nodes must be evaluated at compile time.
        /// </summary>
        CompileTimeOnly,

        /// <summary>
        /// The symbol represents a template that has to be transformed at compile time.
        /// </summary>
        Template
    }
}