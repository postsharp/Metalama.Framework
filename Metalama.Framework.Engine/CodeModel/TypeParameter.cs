// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.References;
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
        private readonly ITypeParameterSymbol _typeSymbol;

        ITypeSymbol ISdkType.TypeSymbol => this._typeSymbol;

        internal TypeParameter( ITypeParameterSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this._typeSymbol = typeSymbol;
        }

        public TypeKind TypeKind => TypeKind.TypeParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => this.Compilation.Factory.GetReflectionType( this._typeSymbol );

        public bool? IsReferenceType => this.IsReferenceTypeImpl();

        public bool? IsNullable => this.IsNullableImpl();

        public string Name => this._typeSymbol.Name;

        public int Index => this._typeSymbol.Ordinal;

        [Memo]
        public IReadOnlyList<IType> TypeConstraints
            => this._typeSymbol.ConstraintTypes.Select( t => this.Compilation.Factory.GetIType( t ) ).ToImmutableArray();

        public TypeKindConstraint TypeKindConstraint
        {
            get
            {
                if ( this._typeSymbol.HasUnmanagedTypeConstraint )
                {
                    return TypeKindConstraint.Unmanaged;
                }
                else if ( this._typeSymbol.HasValueTypeConstraint )
                {
                    return TypeKindConstraint.Struct;
                }
                else if ( this._typeSymbol.HasReferenceTypeConstraint )
                {
                    return TypeKindConstraint.Class;
                }
                else if ( this._typeSymbol.HasNotNullConstraint )
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
            => this._typeSymbol.Variance switch
            {
                Microsoft.CodeAnalysis.VarianceKind.In => VarianceKind.In,
                Microsoft.CodeAnalysis.VarianceKind.Out => VarianceKind.Out,
                _ => VarianceKind.None
            };

        public bool? IsConstraintNullable => this._typeSymbol.ReferenceTypeConstraintNullableAnnotation.ToIsAnnotated();

        public bool HasDefaultConstructorConstraint => this._typeSymbol.HasConstructorConstraint;

        [Memo]
        public override IDeclaration ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this._typeSymbol.ContainingSymbol );

        public override DeclarationKind DeclarationKind => DeclarationKind.TypeParameter;

        public override IGenericContext GenericContext => this.ContainingDeclaration.GenericContext;

        public override ISymbol Symbol => this._typeSymbol;

        public override bool CanBeInherited => ((IDeclarationImpl) this.ContainingDeclaration).CanBeInherited;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => throw new NotImplementedException();

        DeclarationKind IDeclaration.DeclarationKind => DeclarationKind.TypeParameter;

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
        private IRef<ITypeParameter> Ref => this.RefFactory.FromSymbol<ITypeParameter>( this._typeSymbol );

        private protected override IRef<IDeclaration> ToDeclarationRef() => this.Ref;

        IRef<ITypeParameter> ITypeParameter.ToRef() => this.Ref;

        IRef<IType> IType.ToRef() => this.Ref;
    }
}