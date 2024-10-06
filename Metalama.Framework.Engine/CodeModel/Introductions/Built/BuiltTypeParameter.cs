// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Built;

internal sealed class BuiltTypeParameter : BuiltDeclaration, ITypeParameter
{
    private readonly TypeParameterBuilderData _typeParameterBuilder;

    public BuiltTypeParameter( TypeParameterBuilderData builder, CompilationModel compilation, IGenericContext genericContext ) : base(
        compilation,
        genericContext )
    {
        this._typeParameterBuilder = builder;
    }

    public override DeclarationBuilderData BuilderData => this._typeParameterBuilder;

    public TypeKind TypeKind => TypeKind.TypeParameter;

    public SpecialType SpecialType => SpecialType.None;

    public Type ToType() => throw new NotImplementedException();

    public bool? IsReferenceType => this._typeParameterBuilder.IsReferenceType;

    public bool? IsNullable => this._typeParameterBuilder.IsNullable;

    bool IType.Equals( SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => otherType is BuiltTypeParameter otherBuildTypeParameter && otherBuildTypeParameter.BuilderData == this.BuilderData;

    ICompilation ICompilationElement.Compilation => this.Compilation;

    public string Name => this._typeParameterBuilder.Name;

    public int Index => this._typeParameterBuilder.Index;

    [Memo]
    public IReadOnlyList<IType> TypeConstraints => this.MapDeclarationList( this._typeParameterBuilder.TypeConstraints );

    public TypeKindConstraint TypeKindConstraint => this._typeParameterBuilder.TypeKindConstraint;

    public VarianceKind Variance => this._typeParameterBuilder.Variance;

    public bool? IsConstraintNullable => this._typeParameterBuilder.IsConstraintNullable;

    public bool HasDefaultConstructorConstraint => this._typeParameterBuilder.HasDefaultConstructorConstraint;

    private IRef<ITypeParameter> Ref => this.RefFactory.FromBuilt<ITypeParameter>( this );

    IRef<ITypeParameter> ITypeParameter.ToRef() => this.Ref;

    IRef<IType> IType.ToRef() => this.Ref;

    private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public bool Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

    public override int GetHashCode() => this._typeParameterBuilder.GetHashCode();

    public override bool CanBeInherited => ((IDeclarationImpl) this.ContainingDeclaration.AssertNotNull()).CanBeInherited;

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => throw new NotImplementedException();
}