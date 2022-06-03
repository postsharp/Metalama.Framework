using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.PromoteIntroduced;

[assembly: AspectOrder( typeof(PromoteAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.PromoteIntroduced
{
    [AttributeUsage( AttributeTargets.Class )]
    public class PromoteAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override( builder.Target.Fields.Single(), nameof(Template) );
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

    [AttributeUsage( AttributeTargets.Class )]
    public class IntroductionAttribute : TypeAspect
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
            Console.WriteLine( "Original code." );
        }
    }
}