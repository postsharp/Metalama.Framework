// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class BuiltTypeParameter : BuiltDeclaration, ITypeParameter
{
    private readonly TypeParameterBuilder _typeParameterBuilder;

    public BuiltTypeParameter( TypeParameterBuilder builder, CompilationModel compilation ) : base( compilation, builder )
    {
        this._typeParameterBuilder = builder;
    }

    public override DeclarationBuilder Builder => this._typeParameterBuilder;

    public TypeKind TypeKind => TypeKind.TypeParameter;

    public SpecialType SpecialType => SpecialType.None;

    public Type ToType() => throw new NotImplementedException();

    public bool? IsReferenceType => this._typeParameterBuilder.IsReferenceType;

    public bool? IsNullable => this._typeParameterBuilder.IsNullable;

    bool IType.Equals( SpecialType specialType ) => false;

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => otherType is BuiltTypeParameter otherBuildTypeParameter && otherBuildTypeParameter.Builder == this.Builder;

    ICompilation ICompilationElement.Compilation => this.Compilation;

    public string Name => this._typeParameterBuilder.Name;

    public int Index => this._typeParameterBuilder.Index;

    public IReadOnlyList<IType> TypeConstraints => this._typeParameterBuilder.ReadOnlyTypeConstraints;

    public TypeKindConstraint TypeKindConstraint => this._typeParameterBuilder.TypeKindConstraint;

    public VarianceKind Variance => this._typeParameterBuilder.Variance;

    public bool? IsConstraintNullable => this._typeParameterBuilder.IsConstraintNullable;

    public bool HasDefaultConstructorConstraint => this._typeParameterBuilder.HasDefaultConstructorConstraint;

    public bool Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

    public override int GetHashCode() => this._typeParameterBuilder.GetHashCode();

    public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => throw new NotImplementedException();
}