﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class ParameterBuilderData : DeclarationBuilderData
{
    private readonly BuiltDeclarationRef<IParameter> _ref;

    public string? Name { get; }

    public IRef<IType> Type { get; }

    public RefKind RefKind { get; }

    public int Index { get; }

    public TypedConstant? DefaultValue { get; }

    public bool IsParams { get; }

    public ParameterBuilderData( BaseParameterBuilder builder, IFullRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new BuiltDeclarationRef<IParameter>( this, containingDeclaration.RefFactory );

        this.Name = !builder.IsReturnParameter ? builder.Name : "<return>";
        
        this.Type = builder.Type.ToRef();
        this.RefKind = builder.RefKind;
        this.Index = builder.Index;
        this.DefaultValue = builder.DefaultValue;
        this.IsParams = builder.IsParams;
        this.Attributes = builder.Attributes.ToImmutable( this._ref );
    }

    protected override IFullRef<IDeclaration> ToDeclarationRef() => this._ref;

    public override IFullRef<INamedType>? DeclaringType => this.ContainingDeclaration.DeclaringType;

    public new BuiltDeclarationRef<IParameter> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.Parameter;
}