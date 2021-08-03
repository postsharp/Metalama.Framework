// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// A weakly typed base for <see cref="IDeclarationRef{T}"/>.
    /// </summary>
    internal interface IDeclarationRef
    {
        /// <summary>
        /// Gets the target object (typically a symbol or a <see cref="DeclarationBuilder"/>) pointed at by the reference.
        /// </summary>
        object? Target { get; }
    }

    /// <summary>
    /// Represents a reference to a declaration that can be resolved using <see cref="Resolve"/>.
    /// </summary>
    /// <typeparam name="T">The type of the target object of the declaration.</typeparam>
    internal interface IDeclarationRef<out T> : IDeclarationRef
        where T : ICompilationElement
    {
        /// <summary>
        /// Gets the target declaration for a given <see cref="CompilationModel"/>.
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        T Resolve( CompilationModel compilation );

        // This is a temporary method to extract the symbol from the reference, when there is any.
        // In the final implementation, this method should not be necessary.
        ISymbol? GetSymbol( Compilation compilation );
    }
}