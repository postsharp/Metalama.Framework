#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceOverride
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
        public void M()
        {
            Console.WriteLine( "Override." );
            var nic = meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public void M()
        {
            Console.WriteLine( "Source" );
        }
    }
}