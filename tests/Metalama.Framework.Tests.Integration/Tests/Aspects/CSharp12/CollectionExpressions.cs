#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.CollectionExpressions;

public class TheAspect : OverrideMethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        int[] collection = [1, 2, 3, ..Enumerable.Range(3, 2)];
    }

    public override dynamic? OverrideMethod()
    {
        int[] collection1 = [1, 2, 3, ..Enumerable.Range(3, 2)];

        int[] collection2 = meta.CompileTime<int[]>([1, 2, 3, ..Enumerable.Range(3, 2)]);

        int[] collection3 = [1, meta.CompileTime(2), 3, ..Enumerable.Range(3, 2)];

        int[] collection4 = [1, 2, 3, ..meta.CompileTime(Enumerable.Range(3, 2))];

        return meta.Proceed();
    }
}

public class C
{
    [TheAspect]
    static void M()
    {
        int[] collection = [1, 2, ..Enumerable.Range(3, 2)];
    }
}

#endif