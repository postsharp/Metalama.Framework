// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SpecialType = Caravela.Framework.Code.SpecialType;
using TypeKind = Caravela.Framework.Code.TypeKind;
using VarianceKind = Caravela.Framework.Code.VarianceKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class GenericParameter : Declaration, IGenericParameter, ITypeInternal
    {
        private readonly ITypeParameterSymbol _typeSymbol;

        ITypeSymbol? ISdkType.TypeSymbol => this._typeSymbol;

        internal GenericParameter( ITypeParameterSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this._typeSymbol = typeSymbol;
        }

        public TypeKind TypeKind => TypeKind.GenericParameter;

        public SpecialType SpecialType => SpecialType.None;

        public Type ToType() => CompileTimeType.Create( this );

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
                    if ( this._typeSymbol.ReferenceTypeConstraintNullableAnnotation == NullableAnnotation.Annotated )
                    {
                        return TypeKindConstraint.NullableClass;
                    }
                    else
                    {
                        return TypeKindConstraint.Class;
                    }
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

        public bool HasDefaultConstructorConstraint => this._typeSymbol.HasConstructorConstraint;

        [Memo]
        public override IDeclaration ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this._typeSymbol.ContainingSymbol );

        public override DeclarationKind DeclarationKind => DeclarationKind.GenericParameter;

        public override ISymbol Symbol => this._typeSymbol;

        DeclarationKind IDeclaration.DeclarationKind => DeclarationKind.GenericParameter;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        public bool Equals( IType other ) => SymbolEqualityComparer.Default.Equals( this._typeSymbol, ((ITypeInternal) other).TypeSymbol );

        public override string ToString() => this._typeSymbol.ToString();
    }
}