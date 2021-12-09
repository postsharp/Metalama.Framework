// @LiveTemplate

using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.LiveTemplates.LiveTemplate
{
    public class TestAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("This is the overriding method.");
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        public void TargetMethod()
        {
            Console.WriteLine( "This is the original method." );
        }
    }
}
