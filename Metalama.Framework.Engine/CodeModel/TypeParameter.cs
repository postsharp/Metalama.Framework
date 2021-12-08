// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;
using VarianceKind = Metalama.Framework.Code.VarianceKind;

namespace Metalama.Framework.Impl.CodeModel
{
    internal class TypeParameter : Declaration, ITypeParameter, ITypeInternal
    {
        private readonly ITypeParameterSymbol _typeSymbol;

        ITypeSymbol? ISdkType.TypeSymbol => this._typeSymbol;

        internal TypeParameter( ITypeParameterSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this._typeSymbol = typeSymbol;
        }

        public TypeKind TypeKind => TypeKind.GenericParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => this.GetCompilationModel().Factory.GetReflectionType( this._typeSymbol );

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

        public bool? IsConstraintNullable
            => this._typeSymbol.ReferenceTypeConstraintNullableAnnotation switch
            {
                NullableAnnotation.Annotated => true,
                NullableAnnotation.NotAnnotated => false,
                _ => null
            };

        public bool HasDefaultConstructorConstraint => this._typeSymbol.HasConstructorConstraint;

        [Memo]
        public override IDeclaration ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this._typeSymbol.ContainingSymbol );

        public override DeclarationKind DeclarationKind => DeclarationKind.TypeParameter;

        public override ISymbol Symbol => this._typeSymbol;

        public override bool CanBeInherited => ((IDeclarationImpl) this.ContainingDeclaration).CanBeInherited;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => throw new NotImplementedException();

        DeclarationKind IDeclaration.DeclarationKind => DeclarationKind.TypeParameter;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public bool Equals( IType other ) => SymbolEqualityComparer.Default.Equals( this._typeSymbol, ((ITypeInternal) other).TypeSymbol );

        public override string ToString() => this.ContainingDeclaration + "/" + this.Name;
    }
}