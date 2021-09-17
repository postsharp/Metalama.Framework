using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.ConditionalAccess
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("This is the overriding method.");
            var x = meta.This;
            return meta.Target.Method.Invokers.BaseConditional?.Invoke(x);
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        public int? TargetMethod_Int()
        {
            Console.WriteLine("This is the original method.");
            return 42;
        }
    }
}
