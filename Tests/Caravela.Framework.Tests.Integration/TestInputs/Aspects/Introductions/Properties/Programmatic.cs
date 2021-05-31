using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            //{
            //    var advice = aspectBuilder.AdviceFactory.IntroduceProperty(aspectBuilder.TargetDeclaration, nameof(AutoProperty));
            //    advice.Builder.Accessibility = Accessibility.Public;
            //}

            {
                var property = builder.AdviceFactory.IntroduceProperty(builder.TargetDeclaration, nameof(Property));
                property.Accessibility = Accessibility.Public;
            }

            {
                var property = builder.AdviceFactory.IntroduceProperty(builder.TargetDeclaration, "PropertyFromAccessors", nameof(GetPropertyTemplate), nameof(SetPropertyTemplate) );
                property.Accessibility = Accessibility.Public;
            }

            // TODO: Expression bodied template.
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder) { }
        
        public void BuildAspectClass( IAspectClassBuilder builder ) { }

        //[Template]
        //public int AutoProperty { get; set; }

        [Template]
        public int Property
        {
            get
            {
                Console.WriteLine("Get");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("Set");
                dynamic discard = meta.Proceed();
            }
        }

        [Template]
        public int GetPropertyTemplate()
        {
            Console.WriteLine("Get");
            return meta.Proceed();
        }

        [Template]
        public void SetPropertyTemplate(int value)
        {
            Console.WriteLine("Set");
            dynamic discard = meta.Proceed();
        }
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
