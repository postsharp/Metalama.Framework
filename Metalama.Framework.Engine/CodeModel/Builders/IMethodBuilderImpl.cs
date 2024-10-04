// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IMethodBuilderImpl : IMethodBuilder, IMethodImpl, IMemberBuilderImpl
{
    new TypeParameterBuilderList TypeParameters { get; }

    new BaseParameterBuilder ReturnParameter { get; }
}

internal interface IMemberOrNamedTypeBuilderImpl : IMemberOrNamedTypeBuilder, INamedDeclarationBuilderImpl
{
    bool? HasNewKeyword { get; }
}

internal interface IMemberBuilderImpl : IMemberBuilder, IMemberOrNamedTypeBuilderImpl, IMemberImpl;

internal interface INamedDeclarationBuilderImpl : INamedDeclaration, IDeclarationBuilderImpl;