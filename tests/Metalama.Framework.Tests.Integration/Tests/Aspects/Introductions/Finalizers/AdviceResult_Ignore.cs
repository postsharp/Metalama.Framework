using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Finalizers.AdviceResult_Ignore
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceFinalizer(builder.Target, nameof(Finalizer), whenExists: OverrideStrategy.Ignore);

            if (result.Outcome != Advising.AdviceOutcome.Ignore)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Ignore.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceFinalizer)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceFinalizer.");
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
        public int Finalizer()
        {
            return 42;
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass
    {
        ~TargetClass()
        {
            Console.WriteLine("Original code.");
        }
    }
}
