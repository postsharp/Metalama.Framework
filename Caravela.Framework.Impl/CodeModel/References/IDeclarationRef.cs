// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// A weakly typed base for <see cref="IDeclarationRef{T}"/>.
    /// </summary>
    internal interface IDeclarationRef
    {
        /// <summary>
        /// Gets the target object (typically a symbol or a <see cref="DeclarationBuilder"/>) pointed at by the link.
        /// </summary>
        object? Target { get; }
    }

    /// <summary>
    /// Represents a link that can be resolved to an element of code by <see cref="GetForCompilation"/>.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the link.</typeparam>
    internal interface IDeclarationRef<out T> : IDeclarationRef
        where T : IDeclaration
    {
        /// <summary>
        /// Gets the target code element for a given <see cref="CompilationModel"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        T GetForCompilation( CompilationModel compilation );
    }
}