// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class ParameterBuilderData : DeclarationBuilderData
{
    public ParameterBuilderData( BaseParameterBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration ) { }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;
}