using Metalama.Framework.Fabrics;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.AspectTests.Tests.Options.TypeFabric_NotUnderCurrentDeclaration
{
    // <target>
    [ShowOptionsAspect]
    public class C1
    {
        [ShowOptionsAspect]
        public void M( [ShowOptionsAspect] int p ) { }

        private class Fabric : TypeFabric
        {
            public override void AmendType( ITypeAmender amender )
            {
                amender.Select( t => t.ContainingNamespace ).SetOptions( c => new MyOptions { Value = "Namespace" } );
            }
        }
    }
}