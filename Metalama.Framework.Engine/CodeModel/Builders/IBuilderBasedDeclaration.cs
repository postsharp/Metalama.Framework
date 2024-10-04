// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Builders.Data;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IBuilderBasedDeclaration : IDeclarationImpl
{
    DeclarationBuilderData Builder { get; }
}