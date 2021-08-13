using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.Programmatic
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            {
                var propertyBuilder = builder.AdviceFactory.IntroduceProperty(builder.Target, nameof(AutoProperty));
                propertyBuilder.Accessibility = Accessibility.Public;
            }

            {
                var propertyBuilder = builder.AdviceFactory.IntroduceProperty(builder.Target, nameof(Property));
                propertyBuilder.Accessibility = Accessibility.Public;
            }

            {
                var propertyBuilder = builder.AdviceFactory.IntroduceProperty(builder.Target, "PropertyFromAccessors", nameof(GetPropertyTemplate), nameof(SetPropertyTemplate) );
                propertyBuilder.Accessibility = Accessibility.Public;
            }

            // TODO: Expression bodied template.
        }

        [Template]
        public int AutoProperty { get; set; }

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
                meta.Proceed();
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
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}
