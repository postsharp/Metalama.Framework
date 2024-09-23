// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class TypeParameter : Declaration, ITypeParameter, ITypeImpl
    {
        public ITypeParameterSymbol TypeParameterSymbol { get; }

        ITypeSymbol ISdkType.TypeSymbol => this.TypeParameterSymbol;

        internal TypeParameter( ITypeParameterSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this.TypeParameterSymbol = typeSymbol;
        }

        public TypeKind TypeKind => TypeKind.TypeParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => this.Compilation.Factory.GetReflectionType( this.TypeParameterSymbol );

        public bool? IsReferenceType => this.IsReferenceTypeImpl();

        public bool? IsNullable => this.IsNullableImpl();

        public string Name => this.TypeParameterSymbol.Name;

        public int Index => this.TypeParameterSymbol.Ordinal;

        [Memo]
        public IReadOnlyList<IType> TypeConstraints
            => this.TypeParameterSymbol.ConstraintTypes.Select( t => this.Compilation.Factory.GetIType( t ) ).ToImmutableArray();

        public TypeKindConstraint TypeKindConstraint
        {
            get
            {
                if ( this.TypeParameterSymbol.HasUnmanagedTypeConstraint )
                {
                    return TypeKindConstraint.Unmanaged;
                }
                else if ( this.TypeParameterSymbol.HasValueTypeConstraint )
                {
                    return TypeKindConstraint.Struct;
                }
                else if ( this.TypeParameterSymbol.HasReferenceTypeConstraint )
                {
                    return TypeKindConstraint.Class;
                }
                else if ( this.TypeParameterSymbol.HasNotNullConstraint )
                {
                    return TypeKindConstraint.NotNull;
                }
                else
                {
                    return TypeKindConstraint.None;
                }
            }
        }

        public VarianceKind Variance
            => this.TypeParameterSymbol.Variance switch
            {
                Microsoft.CodeAnalysis.VarianceKind.In => VarianceKind.In,
                Microsoft.CodeAnalysis.VarianceKind.Out => VarianceKind.Out,
                _ => VarianceKind.None
            };

        public bool? IsConstraintNullable => this.TypeParameterSymbol.ReferenceTypeConstraintNullableAnnotation.ToIsAnnotated();

        public bool HasDefaultConstructorConstraint => this.TypeParameterSymbol.HasConstructorConstraint;

        [Memo]
        public override IDeclaration ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this.TypeParameterSymbol.ContainingSymbol );

        public override DeclarationKind DeclarationKind => DeclarationKind.TypeParameter;

        public override ISymbol Symbol => this.TypeParameterSymbol;

        public override bool CanBeInherited => ((IDeclarationImpl) this.ContainingDeclaration).CanBeInherited;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => throw new NotImplementedException();

        DeclarationKind ICompilationElement.DeclarationKind => DeclarationKind.TypeParameter;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public override SyntaxTree? PrimarySyntaxTree => ((IDeclarationImpl) this.ContainingDeclaration).PrimarySyntaxTree;

        bool IType.Equals( SpecialType specialType ) => false;

        public bool Equals( IType? otherType, TypeComparison typeComparison )
            => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

        public bool Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

        public override string ToString() => this.ContainingDeclaration + "/" + this.Name;

        public IType Accept( TypeRewriter visitor ) => visitor.Visit( this );

        public override int GetHashCode() => this.Compilation.CompilationContext.SymbolComparer.GetHashCode( this.Symbol );

        [Memo]
        private IRef<ITypeParameter> Ref => this.RefFactory.FromSymbol<ITypeParameter>( this.TypeParameterSymbol );

        private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

        IRef<ITypeParameter> ITypeParameter.ToRef() => this.Ref;

        IRef<IType> IType.ToRef() => this.Ref;
    }
}