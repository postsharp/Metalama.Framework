﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Finalizers.Introduced_Derived
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var introductionResult = builder.Advice.IntroduceFinalizer(builder.Target, nameof(IntroduceTemplate));
            builder.Advice.Override(introductionResult.Declaration, nameof(OverrideTemplate));
        }

        [Template]
        public dynamic? IntroduceTemplate()
        {
            Console.WriteLine("This is the introduction.");
            return meta.Proceed();
        }

        [Template]
        public dynamic? OverrideTemplate()
        {
            Console.WriteLine("This is the override.");
            return meta.Proceed();
        }
    }

    // <target>
    internal class BaseClass
    {
        ~BaseClass()
        {
            Console.WriteLine($"This is the original finalizer.");
        }
    }

    // <target>
    [Override]
    internal class DerivedClass : BaseClass
    {
    }
}
