using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.MiddleReturn_TemplateAssigns
{
    // Tests override method attribute where target method body contains return from the middle of the method. which forces aspect linker to use jumps to inline the override.
    // Template stores the result into a variable.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine("Begin override.");
            dynamic result = proceed();
            Console.WriteLine("End override.");
            return result;
        }
    }

    #region Target
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
    #endregion
}
