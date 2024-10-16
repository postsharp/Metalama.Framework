// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal sealed class TypeParameterBuilder : DeclarationBuilder, ITypeParameterBuilder, ISdkType
{
    private readonly List<IType> _typeConstraints = new();

    private string _name;

    public string Name
    {
        get => this._name;

        [MemberNotNull( nameof(_name) )]
        set
        {
            this.CheckNotFrozen();

            this._name = value;
        }
    }

    public int Index { get; }

    IReadOnlyList<IType> ITypeParameter.TypeConstraints => this._typeConstraints;

    public IReadOnlyList<IType> ReadOnlyTypeConstraints => this._typeConstraints;

    private TypeKindConstraint _typeKindConstraint;

    public TypeKindConstraint TypeKindConstraint
    {
        get => this._typeKindConstraint;
        set
        {
            this.CheckNotFrozen();

            this._typeKindConstraint = value;
        }
    }

    private bool _allowsRefStruct;

    public bool AllowsRefStruct
    {
        get => this._allowsRefStruct;
        set
        {
            this.CheckNotFrozen();

            this._allowsRefStruct = value;
        }
    }

    private VarianceKind _variance;

    public VarianceKind Variance
    {
        get => this._variance;
        set
        {
            this.CheckNotFrozen();

            this._variance = value;
        }
    }

    private bool? _isConstraintNullable;

    public bool? IsConstraintNullable
    {
        get => this._isConstraintNullable;
        set
        {
            this.CheckNotFrozen();

            this._isConstraintNullable = value;
        }
    }

    private bool _hasDefaultConstructorConstraint;

    public bool HasDefaultConstructorConstraint
    {
        get => this._hasDefaultConstructorConstraint;
        set
        {
            this.CheckNotFrozen();

            this._hasDefaultConstructorConstraint = value;
        }
    }

    public void AddTypeConstraint(IType type)
    {
        this.CheckNotFrozen();

        this._typeConstraints.Add(this.Translate(type));
    }

    public void AddTypeConstraint(Type type) => this.AddTypeConstraint(this.Compilation.Factory.GetTypeByReflectionType(type));

    TypeKind IType.TypeKind => TypeKind.TypeParameter;

    public SpecialType SpecialType => SpecialType.None;

    public Type ToType() => throw new NotImplementedException();

    public bool? IsReferenceType => this.IsReferenceTypeImpl();

    public bool? IsNullable => this.IsNullableImpl();

    ICompilation ICompilationElement.Compilation => this.Compilation;

    public override IDeclaration ContainingDeclaration { get; }

    public override DeclarationKind DeclarationKind => DeclarationKind.TypeParameter;

    public override bool CanBeInherited => ((IDeclarationImpl) this.ContainingDeclaration).CanBeInherited;

    public TypeParameterBuilder( MethodBuilder containingMethod, int index, string name ) : base( containingMethod.ParentAdvice )
    {
        this.ContainingDeclaration = containingMethod;
        this.Index = index;
        this.Name = name;
    }

    public TypeParameterBuilder( NamedTypeBuilder containingType, int index, string name ) : base( containingType.ParentAdvice )
    {
        this.ContainingDeclaration = containingType;
        this.Index = index;
        this.Name = name;
    }

    bool IType.Equals( SpecialType specialType ) => false;

    bool IEquatable<IType>.Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

    public bool Equals( IType? otherType, TypeComparison typeComparison )
        => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

    public ITypeSymbol TypeSymbol => throw new NotSupportedException( "Constructed types involving ITypeParameterBuilder are not supported" );

    [Memo]
    public IRef<ITypeParameter> Ref => this.RefFactory.FromBuilder<ITypeParameter>( this );

    public override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

    public new IRef<ITypeParameter> ToRef() => this.Ref;

    IRef<IType> IType.ToRef() => this.Ref;
}