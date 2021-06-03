// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Types;
using Microsoft.CodeAnalysis;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class ArrayType : RoslynType<IArrayTypeSymbol>, IArrayType
    {
        internal ArrayType( IArrayTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation ) { }

        public override TypeKind TypeKind => TypeKind.Array;

        [Memo]
        public IType ElementType => this.Compilation.Factory.GetIType( this.Symbol.ElementType );

        public int Rank => this.Symbol.Rank;
    }
}