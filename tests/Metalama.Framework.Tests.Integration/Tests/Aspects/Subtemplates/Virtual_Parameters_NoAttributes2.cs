using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_Parameters_NoAttributes2;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");

        CalledTemplate(1, 2);

        return meta.Proceed();
    }

    [Template]
    protected virtual void CalledTemplate(int i, [CompileTime] int j)
    {
        Console.WriteLine($"called template i={i} j={j}");
    }
}

class DerivedAspect : Aspect
{
    public override Task<dynamic?> OverrideAsyncMethod()
    {
        CalledTemplate(3, 4);

        return meta.ProceedAsync();
    }

    protected override void CalledTemplate(int i, int j)
    {
        Console.WriteLine($"called template i={i} j={j}");
    }
}

// <target>
class TargetCode
{
    [Aspect]
    void Method1()
    {
    }

    [DerivedAspect]
    void Method2()
    {
    }

    [DerivedAspect]
    async Task Method3()
    {
        await Task.Yield();
    }
}