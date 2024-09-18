// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface IBuilderRef : IDeclarationRef
{
    IDeclarationBuilder Builder { get; }
}