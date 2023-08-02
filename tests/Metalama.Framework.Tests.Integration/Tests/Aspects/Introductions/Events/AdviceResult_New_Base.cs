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

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Events.AdviceResult_New_Base
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceEvent(builder.Target, nameof(Event), whenExists: OverrideStrategy.New);

            if (result.Outcome != Advising.AdviceOutcome.New)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of New.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceEvent)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceEvent.");
            }

            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (!builder.Advice.MutableCompilation.Comparers.Default.Equals(
                    result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
                    builder.Target.ForCompilation(builder.Advice.MutableCompilation).Events.Single()))
            {
                throw new InvalidOperationException($"Declaration was not correct.");
            }
        }

        [Template]
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine("Aspect code.");
                meta.Proceed();
            }
            remove
            {
                Console.WriteLine("Aspect code.");
                meta.Proceed();
            }
        }
    }

    public class BaseClass
    {
        public virtual event EventHandler Event
        {
            add { }
            remove { }
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass : BaseClass
    {
    }
}
