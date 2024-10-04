// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Collections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class ConstructorBuilderData : MemberBuilderData
{
    public ConstructorBuilderData( ConstructorBuilder builder, IRef<INamedType> containingDeclaration ) : base( builder, containingDeclaration ) { }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

    public ConstructorBuilderData( ConstructorBuilder builder, IRef<IDeclaration> containingDeclaration ) : base(
        builder,
        containingDeclaration )
    {
        var me = this.ToDeclarationRef();

        this.Parameters = builder.Parameters.ToImmutable( me );
    }

    public ImmutableArray<ParameterBuilderData> Parameters { get; }

    public override IRef<IMember>? OverriddenMember => null;

    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => [];
}