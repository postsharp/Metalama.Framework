﻿using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.MultipleReturns_TemplateAssigns
{
    // Tests overrides where the target method contains multiple returns.
    // Template assigns the result to a variable.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine("Begin override.");
            var result = proceed();
            Console.WriteLine("End override.");
            return result;
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void_TwoReturns(int x)
        {
            while (x > 0)
            {
                if (x == 42)
                {
                    return;
                }

                x--;
            }

            if (x > 0)
                return;
        }

        [Override]
        public int TargetMethod_Int_TwoReturns(int x)
        {
            while (x > 0)
            {
                if (x == 42)
                {
                    return 42;
                }

                x--;
            }

            if (x > 0)
                return -1;

            return 0;
        }
    }
}
