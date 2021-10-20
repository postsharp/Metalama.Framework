using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.PromoteIntroduced;
using Caravela.TestFramework;

[assembly: AspectOrder(typeof(PromoteAttribute), typeof(IntroductionAttribute))]

namespace Caravela.Framework.IntegrationTests.Aspects.Introductions.Properties.PromoteIntroduced
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PromoteAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.OverrideFieldOrProperty(builder.Target.Fields.Single(), nameof(Template));
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                return meta.Proceed();
            }

            set
            {
                meta.Proceed();
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public int _field;
    }

    // <target>
    [Introduction]
    [Promote]
    internal class TargetClass
    {
        public void Foo()
        {
            Console.WriteLine("Original code.");
        }
    }
}
