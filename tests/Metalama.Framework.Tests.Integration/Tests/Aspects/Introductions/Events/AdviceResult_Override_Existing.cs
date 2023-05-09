using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Events.AdviceResult_Override_Existing
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceEvent(builder.Target, nameof(Event), whenExists: OverrideStrategy.Override);

            if (result.Outcome != Advising.AdviceOutcome.Override)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Override.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceEvent)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceEvent.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (!builder.Target.Compilation.Comparers.Default.Equals(result.Declaration.ForCompilation(builder.Advice.MutableCompilation), builder.Target.Events.Single()))
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

    // <target>
    [TestAspect]
    public class TargetClass
    {
        public event EventHandler Event
        {
            add
            {
                Console.WriteLine("Original code.");
            }
            remove
            {
                Console.WriteLine("Original code.");
            }
        }
    }
}
