// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Builders
{
    internal sealed class BuiltTypeParameter : BuiltDeclaration, ITypeParameter
    {
        public BuiltTypeParameter( TypeParameterBuilder builder, CompilationModel compilation ) : base( compilation, builder )
        {
            this.TypeParameterBuilder = builder;
        }

        public TypeParameterBuilder TypeParameterBuilder { get; }

        public override DeclarationBuilder Builder => this.TypeParameterBuilder;

        public TypeKind TypeKind => TypeKind.TypeParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => throw new NotImplementedException();

        public bool? IsReferenceType => this.TypeParameterBuilder.IsReferenceType;

        public bool? IsNullable => this.TypeParameterBuilder.IsNullable;

        bool IType.Equals( SpecialType specialType ) => false;

        public bool Equals( IType? otherType, TypeComparison typeComparison )
            => otherType is BuiltTypeParameter otherBuildTypeParameter && otherBuildTypeParameter.Builder == this.Builder;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public string Name => this.TypeParameterBuilder.Name;

        public int Index => this.TypeParameterBuilder.Index;

        public IReadOnlyList<IType> TypeConstraints => this.TypeParameterBuilder.ReadOnlyTypeConstraints;

        public TypeKindConstraint TypeKindConstraint => this.TypeParameterBuilder.TypeKindConstraint;

        public VarianceKind Variance => this.TypeParameterBuilder.Variance;

        public bool? IsConstraintNullable => this.TypeParameterBuilder.IsConstraintNullable;

        public bool HasDefaultConstructorConstraint => this.TypeParameterBuilder.HasDefaultConstructorConstraint;

        public bool Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

        public override int GetHashCode() => this.TypeParameterBuilder.GetHashCode();
    }
}