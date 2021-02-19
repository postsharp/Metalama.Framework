using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Constructor : MethodBase, IConstructor, IMemberLink<IConstructor>
    {
        public Constructor( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
        }

        public override CodeElementKind ElementKind => CodeElementKind.Constructor;

        public override bool IsReadOnly => false;

        public override bool IsAsync => false;
        IConstructor ICodeElementLink<IConstructor>.GetForCompilation( CompilationModel compilation ) => this.GetForCompilation<IConstructor>( compilation );
    }
}