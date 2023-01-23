using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedIntroduction_Value;

[assembly: AspectOrder( typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedIntroduction_Value
{
    [AttributeUsage( AttributeTargets.Class )]
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int Property { get; set; }
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override(
                builder.Target.Properties.OfName( nameof(IntroductionAttribute.Property) ).Single(),
                nameof(PropertyTemplate) );
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                Console.WriteLine( "Override" );

                return meta.Target.FieldOrProperty.Value;
            }

            set
            {
                Console.WriteLine( "Override" );
                meta.Target.FieldOrProperty.Value = value;
            }
        }
    }

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass { }
}