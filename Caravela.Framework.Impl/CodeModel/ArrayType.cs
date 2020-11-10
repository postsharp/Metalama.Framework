﻿using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    class ArrayType : IArrayType, ITypeInternal
    {
        private readonly IArrayTypeSymbol _typeSymbol;

        ITypeSymbol ITypeInternal.TypeSymbol => this._typeSymbol;

        private readonly SourceCompilation _compilation;

        internal ArrayType( IArrayTypeSymbol typeSymbol, SourceCompilation compilation )
        {
            this._typeSymbol = typeSymbol;
            this._compilation = compilation;
        }

        [Memo]
        public IType ElementType => this._compilation.SymbolMap.GetIType( this._typeSymbol.ElementType );

        public int Rank => this._typeSymbol.Rank;

        public bool Is( IType other ) =>
            this._compilation.RoslynCompilation.HasImplicitConversion( this._typeSymbol, other.GetSymbol() );

        public bool Is( Type other ) =>
            this.Is( this._compilation.GetTypeByReflectionType( other ) ?? throw new ArgumentException( $"Could not resolve type {other}.", nameof( other ) ) );

        public IArrayType MakeArrayType( int rank = 1 ) =>
            (IArrayType) this._compilation.SymbolMap.GetIType( this._compilation.RoslynCompilation.CreateArrayTypeSymbol( this._typeSymbol, rank ) );

        public override string ToString() => this._typeSymbol.ToString();
    }
}
