// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class ArrayType : RoslynType<IArrayTypeSymbol>, IArrayType
    {
        internal ArrayType( IArrayTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation )
        {
            // Array types with lower bounds or sizes specified are not supported.
            Invariant.Assert( typeSymbol.LowerBounds.IsDefault );
            Invariant.Assert( typeSymbol.Sizes.IsEmpty );

            // MDArrays with rank 1 are not supported.
            Invariant.Implies( typeSymbol.Rank == 1, typeSymbol.IsSZArray );
        }

        internal ITypeImpl WithElementType( IType elementType )
        {
            if ( elementType == this.ElementType )
            {
                return this;
            }
            else
            {
                var symbol =
                    this.Compilation
                        .RoslynCompilation.CreateArrayTypeSymbol(
                            elementType.GetSymbol().AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ),
                            this.Rank );

                return (ITypeImpl) this.Compilation.Factory.GetIType( symbol );
            }
        }

        public override TypeKind TypeKind => TypeKind.Array;

        [Memo]
        public IType ElementType => this.Compilation.Factory.GetIType( this.Symbol.ElementType );

        public int Rank => this.Symbol.Rank;

        public override IType Accept( TypeRewriter visitor ) => visitor.Visit( this );
    }
}