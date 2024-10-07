// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Introductions.Data;

namespace Metalama.Framework.Engine.CodeModel.References;

/// <summary>
/// A non-generic base interface for <see cref="BuiltDeclarationRef{T}"/>.
/// </summary>
internal interface IBuiltDeclarationRef : IRefImpl 
{
    DeclarationBuilderData BuilderData { get; }
}