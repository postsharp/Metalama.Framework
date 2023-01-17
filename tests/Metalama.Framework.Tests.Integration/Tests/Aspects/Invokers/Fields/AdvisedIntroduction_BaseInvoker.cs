﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_BaseInvoker;

[assembly: AspectOrder( typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_BaseInvoker
{
    [AttributeUsage( AttributeTargets.Class )]
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int Field;
    }

    [AttributeUsage( AttributeTargets.Class )]
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advise.Override(
                builder.Target.Fields.OfName( nameof(IntroductionAttribute.Field) ).Single(),
                nameof(PropertyTemplate) );
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                Console.WriteLine("Override");
                return meta.Target.FieldOrProperty.Invokers.Base!.GetValue( meta.This );
            }

            set
            {
                Console.WriteLine("Override");
                meta.Target.FieldOrProperty.Invokers.Base!.SetValue( meta.This, value );
            }
        }
    }

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass { }
}