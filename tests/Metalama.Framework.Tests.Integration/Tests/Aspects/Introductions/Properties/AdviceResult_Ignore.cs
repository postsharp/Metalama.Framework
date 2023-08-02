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

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.AdviceResult_Ignore
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceProperty(builder.Target, nameof(Property), whenExists: OverrideStrategy.Ignore);

            if (result.Outcome != Advising.AdviceOutcome.Ignore)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Ignored.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceProperty)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceProperty.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (result.Declaration != builder.Target.Properties.Single().ForCompilation(result.Declaration.Compilation))
            {
                throw new InvalidOperationException($"Declaration was not correct.");
            }
        }

        [Template]
        public int Property
        {
            get => 42;
            set { }
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
