// @LiveTemplate

using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.SimpleExpressionBodyLive
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

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
            => Console.WriteLine("This is the original method.");
    }
}
