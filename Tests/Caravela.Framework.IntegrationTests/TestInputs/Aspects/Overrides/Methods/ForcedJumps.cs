using System;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Aspects.UnitTests.Overrides.Methods.ForcedJumps
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
            Console.WriteLine("This is beginning of the overriding method.");
            dynamic result = proceed();
            Console.WriteLine("This is end of the overriding method.");
            return result;
        }
    }

    #region Target
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void_NonTrivialFlow(int x)
        {
            if (x == 0)
                return;

            Console.WriteLine("This is target method");
        }

        [Override]
        public int TargetMethod_Int_NonTrivialFlow(int x)
        {
            if (x == 0)
                return 42;

            Console.WriteLine("This is target method");

            return x;
        }
    }
    #endregion
}
