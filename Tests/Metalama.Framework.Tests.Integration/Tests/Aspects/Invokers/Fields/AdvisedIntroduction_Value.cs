// @Skipped(#29134 - Invokers.Base is null for an override aspect applied to a field)

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_Value;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_Value
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestIntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int Field;
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.Override(
                builder.Target.Fields.OfName( nameof(TestIntroductionAttribute.Field) ).Single(),
                nameof(PropertyTemplate) );
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                return meta.Target.FieldOrProperty.Value;
            }

            set
            {
                meta.Target.FieldOrProperty.Value = value;
            }
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}