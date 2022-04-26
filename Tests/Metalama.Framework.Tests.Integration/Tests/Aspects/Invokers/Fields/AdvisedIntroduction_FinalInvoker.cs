using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_FinalInvoker;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_FinalInvoker
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
            builder.Advice.Override(
                builder.Target.Fields.OfName( nameof(TestIntroductionAttribute.Field) ).Single(),
                nameof(PropertyTemplate) );
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                return meta.Target.FieldOrProperty.Invokers.Final.GetValue( meta.This );
            }

            set
            {
                meta.Target.FieldOrProperty.Invokers.Final.SetValue( meta.This, value );
            }
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}