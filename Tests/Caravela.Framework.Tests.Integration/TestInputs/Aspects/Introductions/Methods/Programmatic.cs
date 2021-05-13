using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            {
                var advice = aspectBuilder.AdviceFactory.IntroduceMethod(aspectBuilder.TargetDeclaration, nameof(Template));
                advice.Builder.Name = "IntroducedMethod_Parameters";
                advice.Builder.AddParameter("x", typeof(int));
                advice.Builder.AddParameter("y", typeof(int));
            }

            {
                var advice = aspectBuilder.AdviceFactory.IntroduceMethod(aspectBuilder.TargetDeclaration, nameof(Template));
                advice.Builder.Name = "IntroducedMethod_ReturnType";
                advice.Builder.ReturnType = advice.Builder.Compilation.TypeFactory.GetTypeByReflectionType(typeof(int));
            }

            {
                var advice = aspectBuilder.AdviceFactory.IntroduceMethod(aspectBuilder.TargetDeclaration, nameof(Template));
                advice.Builder.Name = "IntroducedMethod_Accessibility";
                advice.Builder.Accessibility = Accessibility.Private;
            }

            {
                var advice = aspectBuilder.AdviceFactory.IntroduceMethod(aspectBuilder.TargetDeclaration, nameof(Template));
                advice.Builder.Name = "IntroducedMethod_IsStatic";
                advice.Builder.IsStatic = true;
            }

            {
                var advice = aspectBuilder.AdviceFactory.IntroduceMethod(aspectBuilder.TargetDeclaration, nameof(Template));
                advice.Builder.Name = "IntroducedMethod_IsVirtual";
                advice.Builder.IsVirtual = true;
            }

            // TODO: Other members.
        }

        [IntroduceMethodTemplate]
        public dynamic Template()
        {
            Console.WriteLine("This is introduced method.");
            return meta.Proceed();
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
