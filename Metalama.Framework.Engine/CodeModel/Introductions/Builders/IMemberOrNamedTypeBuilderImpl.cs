// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal interface IMemberOrNamedTypeBuilderImpl : IMemberOrNamedTypeBuilder, INamedDeclarationBuilderImpl
{
    bool? HasNewKeyword { get; }
}