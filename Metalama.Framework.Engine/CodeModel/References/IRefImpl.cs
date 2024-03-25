// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;

// ReSharper disable UnusedMemberInSuper.Global

namespace Metalama.Framework.Engine.CodeModel.References
{
    /// <summary>
    /// A weakly typed base for <see cref="ISdkRef{T}"/>.
    /// </summary>
    internal interface IRefImpl
    {
        // TODO: the target must be made a private implementation detail, but many linker tests rely on it.

        /// <summary>
        /// Gets the target object (typically a symbol or an <see cref="IDeclarationBuilder"/>) pointed at by the reference.
        /// </summary>
        object? Target { get; }

        bool IsDefault { get; }

        ISymbol GetClosestSymbol( CompilationContext compilationContext );
    }

    internal interface IRefImpl<out T> : ISdkRef<T>, IRefImpl
        where T : class, ICompilationElement;
}