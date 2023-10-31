using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

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