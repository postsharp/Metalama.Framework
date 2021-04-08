using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.DeclarativeNonVoid
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
        }

        [IntroduceMethod]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine("This is introduced method.");
            var nic = proceed();
        }

        [IntroduceMethod]
        public int IntroducedMethod_Int()
        {
            Console.WriteLine("This is introduced method.");
            return proceed();
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
