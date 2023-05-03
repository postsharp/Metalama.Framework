using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Fields.AdviceResult_New_Base
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result = builder.Advice.IntroduceField(builder.Target, nameof(Field), whenExists: OverrideStrategy.New);

            if (result.Outcome != Advising.AdviceOutcome.New)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of New.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceField)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceField.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (!builder.Target.Compilation.Comparers.Default.Equals(
                    result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
                    builder.Target.ForCompilation(builder.Advice.MutableCompilation).Fields.Single()))
            {
                throw new InvalidOperationException($"Declaration was not correct.");
            }
        }

        [Template]
        public int Field;
    }

    public class BaseClass
    {
        public int Field;
    }

    // <target>
    [TestAspect]
    public class TargetClass : BaseClass
    {
    }
}
