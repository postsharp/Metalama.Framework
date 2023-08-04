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

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Indexers.AdviceResult_Ignore
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceIndexer(builder.Target, typeof(int), nameof(GetTemplate), nameof(SetTemplate), whenExists: OverrideStrategy.Ignore);

            if (result.Outcome != Advising.AdviceOutcome.Ignore)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Ignore.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceIndexer)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceIndexer.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (result.Declaration != builder.Target.Indexers.Single().ForCompilation(result.Declaration.Compilation))
            {
                throw new InvalidOperationException($"Declaration was not correct.");
            }
        }

        [Template]
        public int GetTemplate(int index)
        {
            Console.WriteLine("Aspect code.");
            return meta.Proceed();
        }

        [Template]
        public void SetTemplate(int index, int value)
        {
            Console.WriteLine("Aspect code.");
            meta.Proceed();
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass
    {
        public int this[int index]
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
