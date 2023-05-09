using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Operators.AdviceResult_New_Existing
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
                    whenExists: OverrideStrategy.New);

            if (result.Outcome != Advising.AdviceOutcome.Error)
            {
                throw new InvalidOperationException($"Outcome was {result.Outcome} instead of Error.");
            }

            if (result.AdviceKind != Advising.AdviceKind.IntroduceOperator)
            {
                throw new InvalidOperationException($"AdviceKind was {result.AdviceKind} instead of IntroduceOperator.");
            }
            
            if (result.AspectBuilder != builder)
            {
                throw new InvalidOperationException($"AspectBuilder was not the correct instance.");
            }


            // TODO: #33060
            //if (!builder.Target.Compilation.Comparers.Default.Equals(
            //        result.Declaration.ForCompilation(builder.Advice.MutableCompilation), 
            //        builder.Target.Methods.OfName("op_Addition").Single()))
            //{
            //    throw new InvalidOperationException($"Declaration was not correct.");
            //}
        }

        [Template]
        public int Operator(dynamic? x, dynamic? y)
        {
            Console.WriteLine("Aspect code.");
            return meta.Proceed();
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
