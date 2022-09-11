﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.FieldPromotion_FinalInvoker;

[assembly: AspectOrder(typeof(OverrideAttribute), typeof(PromoteAttribute))]

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

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.Override(builder.Target.FieldsAndProperties.OfName( nameof(TargetClass.Field) ).Single(), nameof(PropertyTemplate));
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                Console.WriteLine("Override.");
                return meta.Target.FieldOrProperty.Invokers.Final!.GetValue(meta.This);
            }

            set
            {
                Console.WriteLine("Override.");
                meta.Target.FieldOrProperty.Invokers.Final!.SetValue(meta.This, value);
            }
        }
    }

    // <target>
    [Promote]
    [Override]
    internal class TargetClass
    {
        public int Field;
    }
}