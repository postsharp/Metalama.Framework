using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable CS0618 // IAdviceResult.AspectBuilder is obsolete

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Finalizers.AdviceResult_Override_Base
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceFinalizer(builder.Target, nameof(Finalizer), whenExists: OverrideStrategy.Override);

            if (result.Outcome != Advising.AdviceOutcome.Default)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Default.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceFinalizer)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceFinalizer.");
            }

            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (!builder.Advice.MutableCompilation.Comparers.Default.Equals(
                    result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
                    builder.Target.ForCompilation(builder.Advice.MutableCompilation).Finalizer))
            {
                throw new InvalidOperationException($"Declaration was not correct.");
            }
        }

        [Template]
        public int Finalizer()
        {
            Console.WriteLine("Aspect code.");
            return meta.Proceed();
        }
    }

    public class BaseClass
    {
        ~BaseClass()
        {
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass : BaseClass
    {
    }
}
