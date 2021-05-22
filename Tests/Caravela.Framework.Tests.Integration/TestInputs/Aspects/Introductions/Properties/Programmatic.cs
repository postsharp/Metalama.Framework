﻿using System;
using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
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
                var advice = builder.AdviceFactory.IntroduceProperty(builder.TargetDeclaration, nameof(Property));
                advice.Builder.Accessibility = Accessibility.Public;
            }

            {
                var advice = builder.AdviceFactory.IntroduceProperty(builder.TargetDeclaration, "PropertyFromAccessors", nameof(GetPropertyTemplate), nameof(SetPropertyTemplate) );
                advice.Builder.Accessibility = Accessibility.Public;
            }

            // TODO: Expression bodied template.
        }

        //[IntroducePropertyTemplate]
        //public int AutoProperty { get; set; }

        [IntroducePropertyTemplate]
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

        [IntroducePropertyGetTemplate]
        public int GetPropertyTemplate()
        {
            Console.WriteLine("Get");
            return meta.Proceed();
        }

        [IntroducePropertySetTemplate]
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
