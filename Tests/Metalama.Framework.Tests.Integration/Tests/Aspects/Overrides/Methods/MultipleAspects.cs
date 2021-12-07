using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.Simple_TwoOverrides;
using Metalama.TestFramework;

[assembly: AspectOrder(typeof(OuterOverrideAttribute), typeof(InnerOverrideAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.Simple_TwoOverrides
{
    // Tests two OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class InnerOverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("This is the inner overriding template method.");
            return meta.Proceed();
        }
    }

    public class OuterOverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("This is the outer overriding template method.");
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [InnerOverride]
        [OuterOverride]
        public void TargetMethod_Void()
        {
            Console.WriteLine("This is the original method.");
        }

        [InnerOverride]
        [OuterOverride]
        public void TargetMethod_Void(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
        }

        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_Int()
        {
            Console.WriteLine("This is the original method.");
            return 42;
        }

        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_Int(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
            return x + y;
        }
    }
}
