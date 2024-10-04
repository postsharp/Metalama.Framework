// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Data;

internal class TypeParameterBuilderData : NamedDeclarationBuilderData
{
    public TypeParameterBuilderData( TypeParameterBuilder builder, IRef<IDeclaration> containingDeclaration ) : base( builder, containingDeclaration )
    {
        this.Index = builder.Index;
        this.Variance = builder.Variance;
        this.IsConstraintNullable = builder.IsConstraintNullable;
        this.HasDefaultConstructorConstraint = builder.HasDefaultConstructorConstraint;
        this.TypeConstraints = builder.TypeConstraints.SelectAsImmutableArray( t => t.ToRef() );
    }

    public ImmutableArray<IRef<IType>> TypeConstraints { get; }

    public override IRef<IDeclaration> ToDeclarationRef() => throw new NotImplementedException();

    public override DeclarationKind DeclarationKind => DeclarationKind.TypeParameter;

    public int Index { get; }

    public VarianceKind Variance { get; set; }

    public bool? IsConstraintNullable { get; set; }

    public bool HasDefaultConstructorConstraint { get; set; }
}