using System;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.Programmatic
{
    public class OverrideAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var advice = builder.AdviceFactory.OverrideMethod(builder.TargetDeclaration.Methods.OfName("TargetMethod").Single(), nameof(Template));
        }

        [OverrideMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine("This is the overriding method.");
            return meta.Proceed();
        }
    }

    [TestOutput]
    [Override]
    internal class TargetClass
    {
        public void TargetMethod()
        {
            Console.WriteLine("This is the original method.");
        }
    }
}
