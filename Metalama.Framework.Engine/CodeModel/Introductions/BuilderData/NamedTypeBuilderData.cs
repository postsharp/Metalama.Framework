﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.BuilderData;

internal class NamedTypeBuilderData : MemberOrNamedTypeBuilderData
{
    private readonly IntroducedRef<INamedType> _ref;

    public IFullRef<INamedType>? BaseType { get; }

    public ImmutableArray<TypeParameterBuilderData> TypeParameters { get; }

    public ImmutableArray<IFullRef<INamedType>> ImplementedInterfaces { get; }

    // Only classes are supported at the moment, so the following members can return a constant value.

#pragma warning disable CA1822
    public TypeKind TypeKind => TypeKind.Class;

    public bool IsReadOnly => false;

    public bool IsRef => false;
#pragma warning restore CA1822

    public NamedTypeBuilderData( NamedTypeBuilder builder, IFullRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new IntroducedRef<INamedType>( this, containingDeclaration.RefFactory );
        this.BaseType = builder.BaseType?.ToFullRef();
        this.TypeParameters = builder.TypeParameters.ToImmutable( this._ref );
        this.ImplementedInterfaces = builder.ImplementedInterfaces.SelectAsImmutableArray( i => i.ToFullRef() );
        this.Attributes = builder.Attributes.ToImmutable( this._ref );
    }

    protected override IFullRef<IDeclaration> ToDeclarationFullRef() => this._ref;

    public new IFullRef<INamedType> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.NamedType;

    public override IEnumerable<DeclarationBuilderData> GetOwnedDeclarations() => base.GetOwnedDeclarations().Concat( this.TypeParameters );
}