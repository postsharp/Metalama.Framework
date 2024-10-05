// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class TypeParameterBuilderData : NamedDeclarationBuilderData
{
    private readonly IRef<ITypeParameter> _ref;
    
    public int Index { get; }

    public VarianceKind Variance { get; }

    public bool? IsConstraintNullable { get; }

    public bool HasDefaultConstructorConstraint { get; }

    public TypeKindConstraint TypeKindConstraint { get;  }
    
    public ImmutableArray<IRef<IType>> TypeConstraints { get; }
    
    public bool? IsReferenceType
    {
        get;
    }
    
    public bool? IsNullable { get; }

    
    public TypeParameterBuilderData( TypeParameterBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this._ref = new DeclarationBuilderDataRef<ITypeParameter>( this );
        this.Index = builder.Index;
        this.Variance = builder.Variance;
        this.IsConstraintNullable = builder.IsConstraintNullable;
        this.HasDefaultConstructorConstraint = builder.HasDefaultConstructorConstraint;
        this.TypeConstraints = builder.TypeConstraints.SelectAsImmutableArray( t => t.ToRef() );
        this.TypeKindConstraint = builder.TypeKindConstraint;
        this.IsReferenceType = builder.IsReferenceType;
        this.IsNullable = builder.IsNullable;
    }

    
    protected override IRef<IDeclaration> ToDeclarationRef() => this._ref;

    public new IRef<ITypeParameter> ToRef() => this._ref;

    public override DeclarationKind DeclarationKind => DeclarationKind.TypeParameter;

    
}