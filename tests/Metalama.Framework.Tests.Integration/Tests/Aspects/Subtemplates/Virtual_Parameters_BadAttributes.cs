using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_Parameters_BadAttributes;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");

        return meta.Proceed();
    }

    [Template]
    protected virtual void CalledTemplate<T>(int i, int j)
    {
        Console.WriteLine($"called template i={i} j={j}");
    }
}

class DerivedAspect : Aspect
{
    public override Task<dynamic?> OverrideAsyncMethod()
    {
        CalledTemplate<int>(3, 4);

        return meta.ProceedAsync();
    }

    protected override void CalledTemplate<[CompileTime] T>(int i, [CompileTime] int j)
    {
        Console.WriteLine($"derived template i={i} j={j}");
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