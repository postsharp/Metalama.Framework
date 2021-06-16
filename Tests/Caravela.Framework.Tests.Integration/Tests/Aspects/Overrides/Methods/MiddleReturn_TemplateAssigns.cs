using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.MiddleReturn_TemplateAssigns
{
    // Tests override method attribute where target method body contains return from the middle of the method. which forces aspect linker to use jumps to inline the override.
    // Template stores the result into a variable.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Begin override.");
            dynamic? result = meta.Proceed();
            Console.WriteLine("End override.");
            return result;
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void(int x)
        {
            Console.WriteLine("Begin target.");

            if (x == 0)
                return;

            Console.WriteLine("End target.");
        }

        [Override]
        public int TargetMethod_Int(int x)
        {
            Console.WriteLine("Begin target.");

            if (x == 0)
                return 42;

            Console.WriteLine("End target.");

            return x;
        }
    }
}
