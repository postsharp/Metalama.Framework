﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Finalizers.Introduced
{
    /*
     * Tests overriding an introduced finalizer works properly.
     */

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
    [Override]
    internal class TargetClass
    {
    }
}
