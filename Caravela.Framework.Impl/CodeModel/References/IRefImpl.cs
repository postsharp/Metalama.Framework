// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Builders;

namespace Caravela.Framework.Impl.CodeModel.References
{
    /// <summary>
    /// A weakly typed base for <see cref="ISdkRef{T}"/>.
    /// </summary>
    internal interface IRefImpl
    {
        // TODO: the target must be made a private implementation detail, but many linker tests rely on it.

        /// <summary>
        /// Gets the target object (typically a symbol or a <see cref="DeclarationBuilder"/>) pointed at by the reference.
        /// </summary>
        object? Target { get; }
    }

    internal interface IRefImpl<out T> : ISdkRef<T>, IRefImpl
        where T : class, ICompilationElement
    { }
}