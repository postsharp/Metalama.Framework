// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Introductions.Data;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal sealed class TypeParameterBuilder : NamedDeclarationBuilder, ITypeParameterBuilder, ISdkType
{
    private readonly List<IType> _typeConstraints = [];

    public int Index { get; }

    public IReadOnlyList<IType> TypeConstraints => this._typeConstraints;

    public IReadOnlyList<IType> ReadOnlyTypeConstraints => this._typeConstraints;

    public TypeKindConstraint TypeKindConstraint { get; set; }

    public VarianceKind Variance { get; set; }

    public bool? IsConstraintNullable { get; set; }

    public bool HasDefaultConstructorConstraint { get; set; }

    public void AddTypeConstraint( IType type ) => this._typeConstraints.Add( this.Translate( type ) );

    public void AddTypeConstraint( Type type ) => this._typeConstraints.Add( this.Compilation.Factory.GetTypeByReflectionType( type ) );

    TypeKind IType.TypeKind => TypeKind.TypeParameter;

    public SpecialType SpecialType => SpecialType.None;

    public Type ToType() => throw new NotImplementedException();

    public bool? IsReferenceType => this.IsReferenceTypeImpl();

    public bool? IsNullable => this.IsNullableImpl();

    ICompilation ICompilationElement.Compilation => this.Compilation;

    public override bool IsDesignTimeObservable => true;

    public override IDeclaration ContainingDeclaration { get; }

    public override DeclarationKind DeclarationKind => DeclarationKind.TypeParameter;

    public override bool CanBeInherited => ((IDeclarationImpl) this.ContainingDeclaration).CanBeInherited;

    public TypeParameterBuilder( MethodBuilder containingMethod, int index, string name ) : base( containingMethod.ParentAdvice, name )
    {
        this.ContainingDeclaration = containingMethod;
        this.Index = index;
    }

    public TypeParameterBuilder( NamedTypeBuilder containingType, int index, string name ) : base( containingType.ParentAdvice, name )
    {
        this.ContainingDeclaration = containingType;
        this.Index = index;
    }

    bool IType.Equals( SpecialType specialType ) => false;

    bool IEquatable<IType>.Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public ITypeSymbol TypeSymbol => throw new NotSupportedException( "Constructed types involving ITypeParameterBuilder are not supported" );

    IRef<ITypeParameter> ITypeParameter.ToRef() => this.Immutable.ToRef();

    IRef<IType> IType.ToRef() => this.Immutable.ToRef();

    [Memo]
    public TypeParameterBuilderData Immutable => new( this.AssertFrozen(), this.ContainingDeclaration.ToFullRef() );
}