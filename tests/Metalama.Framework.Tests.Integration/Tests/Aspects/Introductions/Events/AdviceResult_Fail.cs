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

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Events.AdviceResult_Fail
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceEvent(builder.Target, nameof(Event), whenExists: OverrideStrategy.Fail);

            if (result.Outcome != Advising.AdviceOutcome.Error)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Ignored.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceEvent)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceEvent.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            // TODO: #33060
            //if (result.Declaration != builder.Target.Events.Single())
            //{
            //    throw new InvalidOperationException($"Declaration was not correct.");
            //}
        }

        [Template]
        public event EventHandler Event
        {
            add { }
            remove { }
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass
    {
        public event EventHandler Event
        {
            add { }
            remove { }
        }
    }
}
