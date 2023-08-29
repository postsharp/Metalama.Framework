using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_Parameters;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");

        CalledTemplate<int>(1, 2, 3);

        return meta.Proceed();
    }

    [Template]
    protected virtual void CalledTemplate<[CompileTime] T>(int i, [CompileTime] int j, T k)
    {
        Console.WriteLine($"called template i={i} j={j} k={k}");
    }
}

class DerivedAspect : Aspect
{
    public override Task<dynamic?> OverrideAsyncMethod()
    {
        CalledTemplate<int>(4, 5, 6);

        return meta.ProceedAsync();
    }

    protected override void CalledTemplate<[CompileTime] T>(int i, [CompileTime] int j, T k)
    {
        Console.WriteLine($"derived template i={i} j={j} k={k}");
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