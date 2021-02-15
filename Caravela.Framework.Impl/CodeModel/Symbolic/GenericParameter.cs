﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal class GenericParameter : CodeElement, IGenericParameter, ITypeInternal
    {
        private readonly ITypeParameterSymbol _typeSymbol;

        ITypeSymbol ITypeInternal.TypeSymbol => this._typeSymbol;

        internal GenericParameter( ITypeParameterSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this._typeSymbol = typeSymbol;
        }

        public Code.TypeKind TypeKind => Code.TypeKind.GenericParameter;

        public string Name => this._typeSymbol.Name;

        public int Index => this._typeSymbol.Ordinal;

        [Memo]
        public IReadOnlyList<IType> TypeConstraints => this._typeSymbol.ConstraintTypes.Select( t => this.Compilation.GetIType( t ) ).ToImmutableArray();

        public bool IsCovariant => this._typeSymbol.Variance == VarianceKind.Out;

        public bool IsContravariant => this._typeSymbol.Variance == VarianceKind.In;

        public bool HasDefaultConstructorConstraint => this._typeSymbol.HasConstructorConstraint;

        public bool HasReferenceTypeConstraint => this._typeSymbol.HasReferenceTypeConstraint;

        public bool HasNonNullableValueTypeConstraint => this._typeSymbol.HasValueTypeConstraint;

        [Memo]
        public override ICodeElement ContainingElement => this.Compilation.GetNamedTypeOrMethod( this._typeSymbol.ContainingSymbol );

        public override CodeElementKind ElementKind => CodeElementKind.GenericParameter;

        protected internal override ISymbol Symbol => this._typeSymbol;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.GenericParameter;

        public ITypeFactory TypeFactory => this.Compilation;

        public override string ToString() => this._typeSymbol.ToString();
    }
}
