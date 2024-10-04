// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Builders.Collections;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Builders.Data;

internal class MethodBuilderData : MemberBuilderData
{
    public IReadOnlyList<IRef<IMethod>> ExplicitInterfaceImplementations { get; }

    public bool IsReadOnly { get; }

    public bool IsIteratorMethod { get; }

    public ImmutableArray<TypeParameterBuilderData> TypeParameters { get; }

    public override DeclarationKind DeclarationKind => DeclarationKind.Method;

    public ParameterBuilderData ReturnParameter { get; }

    public ImmutableArray<ParameterBuilderData> Parameters { get; }

    public IRef<IMethod>? OverriddenMethod { get; }

    public override IRef<IMember>? OverriddenMember => this.OverriddenMethod;

    public MethodKind MethodKind { get; }

    public OperatorKind OperatorKind { get; }

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
        this.OverriddenMethod = builder.OverriddenMethod?.ToRef();
        this.ExplicitInterfaceImplementations = builder.ExplicitInterfaceImplementations.SelectAsImmutableArray( i => i.ToRef() );
        this.MethodKind = builder.MethodKind;
        this.OperatorKind = builder.OperatorKind;
    }

    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => this.ExplicitInterfaceImplementations;

    [Memo]
    public IRef<IMethod> Ref => this.RefFactory.FromBuilderData<IMethod>( this );

    public override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public new IRef<IMethod> ToRef() => this.Ref;
}