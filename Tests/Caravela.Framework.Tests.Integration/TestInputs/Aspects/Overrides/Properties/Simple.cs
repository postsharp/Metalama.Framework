using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Simple
{
    // Tests single OverrideProperty aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverridePropertyAspect
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

        [Override]
        public static void TargetMethod_Static()
        {
            Console.WriteLine("This is the original static method.");
        }
    }
}
