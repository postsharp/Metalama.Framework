using System.Linq;
using Metalama.Framework.Fabrics;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.NamespaceFabric_
{
    namespace Ns
    {
        public class Fabric : NamespaceFabric
        {
            public override void AmendNamespace( INamespaceAmender amender )
            {
                amender.SetOptions( c => new MyOptions { Value = "Namespace" } );
                amender.Select( c => c.Types.OfName( nameof(C2) ).Single() ).SetOptions( c => new MyOptions { Value = "C2" } );
            }
        }

        // <target>
        [ShowOptionsAspect]
        public class C1
        {
            [ShowOptionsAspect]
            public void M( [ShowOptionsAspect] int p ) { }
        }

        // <target>
        [ShowOptionsAspect]
        public class C2
        {
            [ShowOptionsAspect]
            public void M( [ShowOptionsAspect] int p ) { }
        }
    }

    // <target>
    [ShowOptionsAspect]
    public class C3
    {
        [ShowOptionsAspect]
        public void M( [ShowOptionsAspect] int p ) { }
    }
}