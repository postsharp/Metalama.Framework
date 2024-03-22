﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Linq;

#pragma warning disable CS0618 // IAdviceResult.AspectBuilder is obsolete

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.AdviceResult_Override_Base
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceMethod(builder.Target, nameof(Method), whenExists: OverrideStrategy.Override);

            if (result.Outcome != Advising.AdviceOutcome.Override)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Override.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceMethod)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceMethod.");
            }

            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (!builder.Advice.MutableCompilation.Comparers.Default.Equals(
                    result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
                    builder.Target.ForCompilation(builder.Advice.MutableCompilation).Methods.OfName("Method").Single()))
            {
                throw new InvalidOperationException($"Declaration was not correct.");
            }
        }

        [Template]
        public int Method()
        {
            Console.WriteLine("Aspect code.");
            return meta.Proceed();
        }
    }

    public class BaseClass
    {
        public virtual int Method()
        {
            return 42;
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass : BaseClass
    {
    }
}
