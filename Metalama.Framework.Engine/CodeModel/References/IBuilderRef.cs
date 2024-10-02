// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// A non-generic base interface for <see cref="BuilderRef{T}"/>.
/// </summary>
internal interface IBuilderRef : ICompilationBoundRefImpl
{
    IDeclarationBuilder Builder { get; }
}