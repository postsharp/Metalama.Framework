using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class GenericParameter : CodeElement, IGenericParameter, ITypeInternal
    {
        private readonly ITypeParameterSymbol _typeSymbol;

        ITypeSymbol? ISdkType.TypeSymbol => this._typeSymbol;

        internal GenericParameter( ITypeParameterSymbol typeSymbol, CompilationModel compilation ) : base( compilation )
        {
            this._typeSymbol = typeSymbol;
        }

        public Code.TypeKind TypeKind => Code.TypeKind.GenericParameter;

        public string Name => this._typeSymbol.Name;

        public int Index => this._typeSymbol.Ordinal;

        [Memo]
        public IReadOnlyList<IType> TypeConstraints => this._typeSymbol.ConstraintTypes.Select( t => this.Compilation.Factory.GetIType( t ) ).ToImmutableArray();

        public bool IsCovariant => this._typeSymbol.Variance == VarianceKind.Out;

        public bool IsContravariant => this._typeSymbol.Variance == VarianceKind.In;

        public bool HasDefaultConstructorConstraint => this._typeSymbol.HasConstructorConstraint;

        public bool HasReferenceTypeConstraint => this._typeSymbol.HasReferenceTypeConstraint;

        public bool HasNonNullableValueTypeConstraint => this._typeSymbol.HasValueTypeConstraint;

        [Memo]
        public override ICodeElement ContainingElement => this.Compilation.Factory.GetCodeElement( this._typeSymbol.ContainingSymbol );

        public override CodeElementKind ElementKind => CodeElementKind.GenericParameter;

        public override ISymbol Symbol => this._typeSymbol;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.GenericParameter;

        ITypeFactory IType.TypeFactory => this.Compilation.Factory;

        public bool Equals( IType other ) => SymbolEqualityComparer.Default.Equals( this._typeSymbol, ((ITypeInternal) other).TypeSymbol );

        public override string ToString() => this._typeSymbol.ToString();
    }
}
