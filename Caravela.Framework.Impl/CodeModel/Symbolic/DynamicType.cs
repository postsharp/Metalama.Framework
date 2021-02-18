using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal class DynamicType : RoslynType<IDynamicTypeSymbol>, IDynamicType
    {

        internal DynamicType( IDynamicTypeSymbol typeSymbol, CompilationModel compilation ) : base( typeSymbol, compilation )
        {
        }

        public override Code.TypeKind TypeKind => Code.TypeKind.Dynamic;
    }
}
