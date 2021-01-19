using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    class PointerType : IPointerType, ITypeInternal
    {
        private readonly IPointerTypeSymbol _typeSymbol;

        ITypeSymbol ITypeInternal.TypeSymbol => this._typeSymbol;

        private readonly SourceCompilation _compilation;

        internal PointerType( IPointerTypeSymbol typeSymbol, SourceCompilation compilation )
        {
            this._typeSymbol = typeSymbol;
            this._compilation = compilation;
        }

        public Code.TypeKind Kind => Code.TypeKind.Pointer;

        [Memo]
        public IType PointedAtType => this._compilation.SymbolMap.GetIType( this._typeSymbol.PointedAtType );

        public bool Is( IType other ) =>
            this._compilation.RoslynCompilation.HasImplicitConversion( this._typeSymbol, other.GetSymbol() );

        public bool Is( Type other ) =>
            this.Is( this._compilation.GetTypeByReflectionType( other ) ?? throw new ArgumentException( $"Could not resolve type {other}.", nameof( other ) ) );

        public IArrayType MakeArrayType( int rank = 1 ) =>
            (IArrayType) this._compilation.SymbolMap.GetIType( this._compilation.RoslynCompilation.CreateArrayTypeSymbol( this._typeSymbol, rank ) );

        public IPointerType MakePointerType() =>
            (IPointerType) this._compilation.SymbolMap.GetIType( this._compilation.RoslynCompilation.CreatePointerTypeSymbol( this._typeSymbol ) );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext context = null ) => this._typeSymbol.ToDisplayString();

        public override string ToString() => this._typeSymbol.ToString();
    }
}
