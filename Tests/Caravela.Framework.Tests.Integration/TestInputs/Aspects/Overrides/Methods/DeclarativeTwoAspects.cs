using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.DeclarativeTwoAspects;
using static Caravela.Framework.Aspects.TemplateContext;

[assembly: AspectOrder(typeof(InnerOverrideAttribute), typeof(OuterOverrideAttribute))]

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.DeclarativeTwoAspects
{
    public class InnerOverrideAttribute : Attribute, IAspect<IMethod>
    {
        public void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
        }

        [OverrideMethod]
        public dynamic Template()
        {
            Console.WriteLine("This is the inner overriding template method.");
            return proceed();
        }
    }

    public class OuterOverrideAttribute : Attribute, IAspect<IMethod>
    {
        public void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
        }

        [OverrideMethod]
        public dynamic Template()
        {
            Console.WriteLine("This is the outer overriding template method.");
            return proceed();
        }
    }

    #region Target
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
    #endregion
}
