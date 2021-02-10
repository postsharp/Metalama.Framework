using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class GenericParameter : IGenericParameter
    {
        private readonly ITypeParameterSymbol _typeSymbol;

        public ITypeSymbol TypeSymbol => this._typeSymbol;

        private readonly SourceCompilationModel _compilation;

        internal GenericParameter( ITypeParameterSymbol typeSymbol, SourceCompilationModel compilation )
        {
            this._typeSymbol = typeSymbol;
            this._compilation = compilation;
        }

        public Code.TypeKind TypeKind => Code.TypeKind.GenericParameter;

        public string Name => this._typeSymbol.Name;

        public int Index => this._typeSymbol.Ordinal;

        [Memo]
        public IImmutableList<IType> TypeConstraints => this._typeSymbol.ConstraintTypes.Select( t => this._compilation.SymbolMap.GetIType( t ) ).ToImmutableList();

        public bool IsCovariant => this._typeSymbol.Variance == VarianceKind.Out;

        public bool IsContravariant => this._typeSymbol.Variance == VarianceKind.In;

        public bool HasDefaultConstructorConstraint => this._typeSymbol.HasConstructorConstraint;

        public bool HasReferenceTypeConstraint => this._typeSymbol.HasReferenceTypeConstraint;

        public bool HasNonNullableValueTypeConstraint => this._typeSymbol.HasValueTypeConstraint;

        [Memo]
        public CodeElement? ContainingElement => this._compilation.SymbolMap.GetNamedTypeOrMethod( this._typeSymbol.ContainingSymbol );

        [Memo]
        public IImmutableList<Attribute> Attributes => this._typeSymbol.GetAttributes().Select( a => new Attribute( a, this._compilation.SymbolMap ) ).ToImmutableReactive();

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.GenericParameter;

        public bool Is( IType other ) =>
            this._compilation.RoslynCompilation.HasImplicitConversion( this._typeSymbol, other.GetSymbol() );

        public bool Is( Type other ) =>
            this.Is( this._compilation.GetTypeByReflectionType( other ) ?? throw new ArgumentException( $"Could not resolve type {other}.", nameof( other ) ) );

        public IArrayType MakeArrayType( int rank = 1 ) =>
            (IArrayType) this._compilation.SymbolMap.GetIType( this._compilation.RoslynCompilation.CreateArrayTypeSymbol( this._typeSymbol, rank ) );

        public IPointerType MakePointerType() =>
            (IPointerType) this._compilation.SymbolMap.GetIType( this._compilation.RoslynCompilation.CreatePointerTypeSymbol( this._typeSymbol ) );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null ) => this._typeSymbol.ToDisplayString();

        public override string ToString() => this._typeSymbol.ToString();
    }
}
