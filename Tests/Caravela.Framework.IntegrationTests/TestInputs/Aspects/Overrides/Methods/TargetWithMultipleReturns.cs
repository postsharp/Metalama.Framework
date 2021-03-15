using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Overrides.Methods.TargetWithMultipleReturns
{
    // This tests 

    public class OverrideAttribute : Attribute, IAspect<IMethod>
    {
        public void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
        }

        [OverrideMethod]
        public dynamic Template()
        {
            Console.WriteLine("This is the overriding method.");
            return proceed();
        }
    }

    #region Target
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

            return 0;
        }
    }
    #endregion
}
