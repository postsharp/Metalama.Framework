using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Indexers.AdviceResult_Override
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceIndexer(builder.Target, typeof(int), nameof(GetTemplate), nameof(SetTemplate), whenExists: OverrideStrategy.Override);

            if (result.Outcome != Advising.AdviceOutcome.Default)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Default.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceIndexer)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceIndexer.");
            }

            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (!builder.Advice.MutableCompilation.Comparers.Default.Equals(
                    result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
                    builder.Target.ForCompilation(builder.Advice.MutableCompilation).Indexers.Single()))
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
    }
}
