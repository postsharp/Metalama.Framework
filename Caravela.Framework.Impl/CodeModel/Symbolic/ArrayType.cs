using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using TypeKind = Caravela.Framework.Code.TypeKind;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal class ArrayType : RoslynType<IArrayTypeSymbol>, IArrayType
    {
        internal ArrayType( IArrayTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation )
        {
        }

        public override TypeKind TypeKind => TypeKind.Array;

        [Memo]
        public IType ElementType => this.Compilation.Factory.GetIType( this.Symbol.ElementType );

        public int Rank => this.Symbol.Rank;
    }
}
