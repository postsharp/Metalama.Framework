﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class ArrayType : RoslynType<IArrayTypeSymbol>, IArrayType
    {
        internal ArrayType( IArrayTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        internal ITypeImpl WithElementType( ITypeImpl elementType )
        {
            if ( elementType == this.ElementType )
            {
                return this;
            }
            else
            {
                var symbol = this.GetCompilationModel().RoslynCompilation.CreateArrayTypeSymbol( elementType.GetSymbol(), this.Rank );

                return (ITypeImpl) this.GetCompilationModel().Factory.GetIType( symbol );
            }
        }

        public override TypeKind TypeKind => TypeKind.Array;

        [Memo]
        public IType ElementType => this.Compilation.Factory.GetIType( this.Symbol.ElementType );

        public int Rank => this.Symbol.Rank;

        public override ITypeImpl Accept( TypeRewriter visitor ) => visitor.Visit( this );
    }
}