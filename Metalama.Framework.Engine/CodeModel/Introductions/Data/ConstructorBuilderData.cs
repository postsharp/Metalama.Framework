// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.Introductions.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class ConstructorBuilderData : MemberBuilderData
{
    private readonly IRef<IConstructor> _ref;
    
    public IRef<IConstructor>? ReplacedImplicitConstructor { get; }
    public ImmutableArray<ParameterBuilderData> Parameters { get; }
    
    public ConstructorInitializerKind InitializerKind { get; }
    
    public ImmutableArray<(IExpression Expression, string? ParameterName)> InitializerArguments { get; }

    protected override IRef<IDeclaration> ToDeclarationRef() => this._ref;
    
    public new IRef<IConstructor> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Constructor;

    public ConstructorBuilderData( ConstructorBuilder builder, IRef<IDeclaration> containingDeclaration ) : base(
        builder,
        containingDeclaration )
    {
        this._ref = new DeclarationBuilderDataRef<IConstructor>( this);

        this.Parameters = builder.Parameters.ToImmutable( this._ref );
        this.ReplacedImplicitConstructor = builder.ReplacedImplicitConstructor?.ToRef();
        this.InitializerKind = builder.InitializerKind;
        this.InitializerArguments = builder.InitializerArguments.ToImmutableArray();
    }

    

    public override IRef<IMember>? OverriddenMember => null;

    public override IReadOnlyList<IRef<IMember>> ExplicitInterfaceImplementationMembers => [];

    
}