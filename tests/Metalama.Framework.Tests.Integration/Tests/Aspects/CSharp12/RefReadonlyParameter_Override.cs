#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.RefReadonlyParameter_Override;

class TheAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        foreach (var parameter in meta.Target.Parameters)
        {
            Console.WriteLine($"{parameter}: Kind={parameter.RefKind}, Value={parameter.Value}");
        }

        return meta.Proceed();
    }
}

class C
{
    [TheAspect]
    void M(in int i, ref readonly int j)
    {
        Console.WriteLine(i + j);
    }
}

#endif