using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedIntroduction_BaseInvoker;

[assembly: AspectOrder( typeof(TestAttribute), typeof(TestIntroductionAttribute) )]

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedIntroduction_BaseInvoker
{
    [AttributeUsage( AttributeTargets.Class )]
    public class TestIntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int Property { get; set; }
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.OverrideFieldOrProperty(
                builder.Target.Properties.OfName( nameof(TestIntroductionAttribute.Property) ).Single(),
                nameof(PropertyTemplate) );
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                return meta.Target.FieldOrProperty.Invokers.Base!.GetValue( meta.This );
            }

            set
            {
                meta.Target.FieldOrProperty.Invokers.Base!.SetValue( meta.This, value );
            }
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass { }
}