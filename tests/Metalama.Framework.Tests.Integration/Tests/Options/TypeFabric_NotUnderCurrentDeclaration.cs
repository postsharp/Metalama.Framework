using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

namespace Metalama.Framework.Tests.Integration.Tests.Options.TypeFabric_NotUnderCurrentDeclaration
{
    // <target>
    [OptionsAspect]
    public class C1
    {
        [OptionsAspect]
        public void M( [OptionsAspect] int p ) { }

        private class Fabric : TypeFabric
        {
            public override void AmendType( ITypeAmender amender )
            {
                amender.Outbound.Select( t => t.Namespace ).Configure( c => new MyOptions { Value = "Namespace" } );
            }
        }
    }
}