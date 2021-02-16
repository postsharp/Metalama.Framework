// unset

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal class Constructor : MethodBase, IConstructor
    {
        public Constructor( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
        }

        public override CodeElementKind ElementKind => CodeElementKind.Constructor;

        public override bool IsReadOnly => false;

        public override bool IsAsync => false;
    }
}