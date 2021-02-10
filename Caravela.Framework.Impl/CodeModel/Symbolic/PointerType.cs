using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class PointerType : RoslynType<IPointerTypeSymbol>, IPointerType
    {

        internal PointerType( IPointerTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation)
        {
        }

        public override Code.TypeKind TypeKind => Code.TypeKind.Pointer;

        [Memo]
        public IType PointedAtType => this.Compilation.GetIType( this.Symbol.PointedAtType );

        
    }
}
