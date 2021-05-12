using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void Initialize(IAspectBuilder<INamedType> aspectBuilder)
        {
            {
                var advice = aspectBuilder.AdviceFactory.IntroduceProperty(aspectBuilder.TargetDeclaration, nameof(AutoProperty));
                advice.Builder.Accessibility = Accessibility.Public;
            }

            {
                var advice = aspectBuilder.AdviceFactory.IntroduceProperty(aspectBuilder.TargetDeclaration, nameof(Property));
                advice.Builder.Accessibility = Accessibility.Public;
            }

            {
                var advice = aspectBuilder.AdviceFactory.IntroduceProperty(aspectBuilder.TargetDeclaration, "PropertyFromAccessors", nameof(GetPropertyTemplate), nameof(SetPropertyTemplate) );
                advice.Builder.Accessibility = Accessibility.Public;
            }

            // TODO: Expression bodied template.
        }

        [IntroducePropertyTemplate]
        public int AutoProperty { get; set; }

        [IntroducePropertyTemplate]
        public int Property
        {
            get
            {
                Console.WriteLine("Get");
                return proceed();
            }

            set
            {
                Console.WriteLine("Set");
                dynamic discard = proceed();
            }
        }

        [IntroducePropertyGetTemplate]
        public int GetPropertyTemplate()
        {
            Console.WriteLine("Get");
            return proceed();
        }

        [IntroducePropertySetTemplate]
        public void SetPropertyTemplate(int value)
        {
            Console.WriteLine("Set");
            dynamic discard = proceed();
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
