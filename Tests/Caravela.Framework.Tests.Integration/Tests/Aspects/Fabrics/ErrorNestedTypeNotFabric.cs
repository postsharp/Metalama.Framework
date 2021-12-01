using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Fabrics.ErrorNestedTypeNotFabric
{
    // <target>
    internal class TargetCode
    {
        [CompileTimeOnly]
        private enum E { }

        [CompileTimeOnly]
        private interface I { }

        [CompileTimeOnly]
        private struct S { }

        [CompileTimeOnly]
        private delegate void D();

        [CompileTimeOnly]
        private record R( int x );

        [CompileTimeOnly]
        private class C { }
    }
}