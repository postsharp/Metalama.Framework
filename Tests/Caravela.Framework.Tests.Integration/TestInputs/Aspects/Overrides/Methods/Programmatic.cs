using System;
using System.Linq;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.Programmatic
{
    public class OverrideAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            var advice = aspectBuilder.AdviceFactory.OverrideMethod(aspectBuilder.TargetDeclaration.Methods.OfName("TargetMethod").Single(), nameof(Template));
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
