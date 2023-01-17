﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Finalizers.ExistingConflict_Ignore
{
    /*
     * Tests single introducing a finalizer into a class that already has one a using Ignore conflict behavior ignores the advice.
     */

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var introductionResult = builder.Advice.IntroduceFinalizer( builder.Target, nameof(IntroduceTemplate), whenExists: OverrideStrategy.Ignore );
        }

        [Template]
        public dynamic? IntroduceTemplate()
        {
            Console.WriteLine( "This is the introduction." );

            return meta.Proceed();
        }
    }

    // <target>
    [Override]
    internal class TargetClass
    {
        ~TargetClass()
        {
            Console.WriteLine( "This is the existing finalizer." );
        }
    }
}