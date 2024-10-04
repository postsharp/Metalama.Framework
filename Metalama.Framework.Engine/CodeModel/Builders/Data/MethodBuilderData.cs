// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class MethodBuilderData : MemberBuilderData
{
    public MethodBuilderData( IMethodBuilderImpl builder, IRef<IDeclaration> containingDeclaration ) : base(
        builder,
        containingDeclaration )
    {
        var me = this.ToDeclarationRef();

        this.IsReadOnly = builder.IsReadOnly;
        this.IsIteratorMethod = builder.IsIteratorMethod.AssertNotNull();
        this.TypeParameters = builder.TypeParameters.ToImmutable( me );
        this.Parameters = builder.Parameters.ToImmutable( me );
        this.ReturnParameter = new ParameterBuilderData( builder.ReturnParameter, me );
    }

    public bool IsReadOnly { get; }

    public bool IsIteratorMethod { get; }

    public ImmutableArray<TypeParameterBuilderData> TypeParameters { get; }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Method;

    public ParameterBuilderData ReturnParameter { get; }

    public ImmutableArray<ParameterBuilderData> Parameters { get; }
}