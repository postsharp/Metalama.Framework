// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IBuilderBasedDeclaration : IDeclarationImpl
{
    IDeclarationBuilder Builder { get; }
}