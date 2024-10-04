// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Collections;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal interface IMethodBuilderImpl : IMethodBuilder, IMethodImpl, IMemberBuilderImpl
{
    new TypeParameterBuilderList TypeParameters { get; }

    new BaseParameterBuilder ReturnParameter { get; }
}