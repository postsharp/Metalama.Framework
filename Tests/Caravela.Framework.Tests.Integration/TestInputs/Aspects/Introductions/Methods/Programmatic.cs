using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            var advice = aspectBuilder.AdviceFactory.IntroduceMethod(aspectBuilder.TargetDeclaration, nameof(Template));

            advice.Builder.Name = "IntroducedMethod";
            advice.Builder.ReturnType = advice.Builder.Compilation.TypeFactory.GetTypeByReflectionType(typeof(int));
            advice.Builder.AddParameter("x", typeof(int));
            advice.Builder.AddParameter("y", typeof(int));
        }

        [IntroduceMethodTemplate]
        public dynamic Template()
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
