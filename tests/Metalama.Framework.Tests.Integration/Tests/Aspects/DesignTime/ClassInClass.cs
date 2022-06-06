#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.ClassInClass
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public void M()
        {
            Console.WriteLine( "This is introduced method." );
            var nic = meta.Proceed();
        }
    }

    // <target>
    internal partial class TargetClass
    {
        [Introduction]
        private partial class Nested { }
    }
}