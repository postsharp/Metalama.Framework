using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.Simple
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine("This is the overriding method.");
            return proceed();
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
        {
            Console.WriteLine("This is the original method.");
        }

        [Override]
        public void TargetMethod_Void(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
        }

        [Override]
        public int TargetMethod_Int()
        {
            Console.WriteLine("This is the original method.");
            return 42;
        }

        [Override]
        public int TargetMethod_Int(int x, int y)
        {
            Console.WriteLine($"This is the original method {x} {y}.");
            return x + y;
        }
    }
}
