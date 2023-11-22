﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0618 // IAdviceResult.AspectBuilder is obsolete

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.AdviceResult_New_Existing
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceProperty(builder.Target, nameof(Property), whenExists: OverrideStrategy.New);

            if (result.Outcome != Advising.AdviceOutcome.Error)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Error.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceProperty)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceProperty.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            // TODO: #33060
            //if (!builder.Target.Compilation.Comparers.Default.Equals(
            //        result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
            //        builder.Target.Properties.Single()))
            //{
            //    throw new InvalidOperationException($"Declaration was not correct.");
            //}
        }

        [Template]
        public int Property
        {
            get
            {
                Console.WriteLine("Aspect code.");
                return meta.Proceed();
            }
            set
            {
                Console.WriteLine("Aspect code.");
                meta.Proceed();
            }
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass
    {
        public int Property
        {
            get
            {
                Console.WriteLine("Original code.");
                return 42;
            }
            set
            {
                Console.WriteLine("Original code.");
            }
        }
    }
}
