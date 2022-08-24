using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.FieldPromotion_FinalInvoker;

[assembly: AspectOrder(typeof(PromoteAttribute), typeof(TestAttribute))]

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.FieldPromotion_FinalInvoker
{
    public class PromoteAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.Override(builder.Target.Fields.OfName("Field").Single(), nameof(PropertyTemplate));
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                Console.WriteLine("This is aspect code.");

                return meta.Proceed();
            }
            set
            {
                Console.WriteLine("This is aspect code.");
                meta.Proceed();
            }
        }
    }

    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.Override(builder.Target.AllFieldsAndProperties.OfName( nameof(TargetClass.Field) ).Single(), nameof(PropertyTemplate));
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                return meta.Target.FieldOrProperty.Invokers.Final!.GetValue(meta.This);
            }

            set
            {
                meta.Target.FieldOrProperty.Invokers.Final!.SetValue(meta.This, value);
            }
        }
    }

    // <target>
    [Promote]
    [Test]
    internal class TargetClass
    {
        public int Field;
    }
}