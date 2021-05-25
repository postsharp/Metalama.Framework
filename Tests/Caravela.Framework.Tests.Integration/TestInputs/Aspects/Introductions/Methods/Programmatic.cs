using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Methods.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            {
                var introduced = builder.AdviceFactory.IntroduceMethod(builder.TargetDeclaration, nameof(Template));
                introduced.Name = "IntroducedMethod_Parameters";
                introduced.AddParameter("x", typeof(int));
                introduced.AddParameter("y", typeof(int));
            }

            {
                var introduced = builder.AdviceFactory.IntroduceMethod(builder.TargetDeclaration, nameof(Template));
                introduced.Name = "IntroducedMethod_ReturnType";
                introduced.ReturnType = introduced.Compilation.TypeFactory.GetTypeByReflectionType(typeof(int));
            }

            {
                var introduced = builder.AdviceFactory.IntroduceMethod(builder.TargetDeclaration, nameof(Template));
                introduced.Name = "IntroducedMethod_Accessibility";
                introduced.Accessibility = Accessibility.Private;
            }

            {
                var introduced = builder.AdviceFactory.IntroduceMethod(builder.TargetDeclaration, nameof(Template));
                introduced.Name = "IntroducedMethod_IsStatic";
                introduced.IsStatic = true;
            }

            {
                var introduced = builder.AdviceFactory.IntroduceMethod(builder.TargetDeclaration, nameof(Template));
                introduced.Name = "IntroducedMethod_IsVirtual";
                introduced.IsVirtual = true;
            }

            // TODO: Other members.
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder) { }

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
