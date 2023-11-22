﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Linq;

#pragma warning disable CS0618 // IAdviceResult.AspectBuilder is obsolete

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Operators.AdviceResult_Ignore
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var result =
                builder.Advice.IntroduceBinaryOperator(
                    builder.Target,
                    nameof(Operator),
                    builder.Target,
                    TypeFactory.GetType(SpecialType.Int32),
                    TypeFactory.GetType(SpecialType.Int32),
                    OperatorKind.Addition, 
                    whenExists: OverrideStrategy.Ignore);

            if (result.Outcome != Advising.AdviceOutcome.Ignore)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Ignored.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceOperator)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceOperator.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }

            if (result.Declaration != builder.Target.Methods.Single().ForCompilation(result.Declaration.Compilation))
            {
                throw new InvalidOperationException($"Declaration was not correct.");
            }
        }

        [Template]
        public int Operator(dynamic? x, dynamic? y)
        {
            return 42;
        }
    }

    // <target>
    [TestAspect]
    public class TargetClass
    {
        public static int operator +(TargetClass x, int y)
        {
            Console.WriteLine("Original code.");
            return y;
        }
    }
}
