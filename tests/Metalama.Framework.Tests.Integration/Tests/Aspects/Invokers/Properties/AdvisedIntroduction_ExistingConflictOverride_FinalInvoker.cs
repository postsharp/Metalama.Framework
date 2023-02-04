﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.AdvisedIntroduction_ExistingConflictOverride_FinalInvoker;
using System;

[assembly: AspectOrder(typeof(IntroductionAttribute), typeof(TestAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.AdvisedIntroduction_ExistingConflictOverride_FinalInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int BaseClassProperty
        {
            get
            {
                Console.WriteLine("This is introduced property.");

                return meta.Proceed();
            }
        }

        [Introduce(WhenExists = OverrideStrategy.Override)]
        public int TargetClassProperty
        {
            get
            {
                Console.WriteLine("This is introduced property.");

                return meta.Proceed();
            }
        }
    }

    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var property in builder.Target.Properties)
            {
                builder.Advice.Override(property, nameof(PropertyTemplate));
            }

        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                return meta.Target.FieldOrProperty.GetValue(meta.This);
            }

            set
            {
                meta.Target.FieldOrProperty.SetValue(meta.This, value);
            }
        }

    }

    internal class BaseClass
    {
        public virtual int BaseClassProperty
        {
            get => 42;
        }
    }

    // <target>
    [Introduction]
    [Test]
    internal class TargetClass : BaseClass
    {
        public int TargetClassProperty
        {
            get => 42;
        }
    }
}
