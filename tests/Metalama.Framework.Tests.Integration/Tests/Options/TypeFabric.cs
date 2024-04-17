using Metalama.Framework.Fabrics;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.TypeFabric_
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
                amender.SetOptions( c => new MyOptions { Value = "Type" } );
            }
        }
    }
}