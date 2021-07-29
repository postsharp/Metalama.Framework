// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.InternalInterfaces;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SpecialType = Caravela.Framework.Code.SpecialType;
using TypeKind = Caravela.Framework.Code.TypeKind;

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

        public bool IsCovariant => this._typeSymbol.Variance == VarianceKind.Out;

        public bool IsContravariant => this._typeSymbol.Variance == VarianceKind.In;

        public bool HasDefaultConstructorConstraint => this._typeSymbol.HasConstructorConstraint;

        public bool HasReferenceTypeConstraint => this._typeSymbol.HasReferenceTypeConstraint;

        public bool HasNonNullableValueTypeConstraint => this._typeSymbol.HasValueTypeConstraint;

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