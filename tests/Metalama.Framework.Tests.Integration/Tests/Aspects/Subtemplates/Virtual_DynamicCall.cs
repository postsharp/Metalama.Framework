using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_DynamicCall;

#pragma warning disable CS1998 // Async method lacks 'await' operators

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("virtual method");

        return meta.Proceed();
    }

    public async override Task<dynamic?> OverrideAsyncMethod()
    {
        Console.WriteLine("normal template");

        if (meta.Target.Parameters["condition"].Value)
        {
            meta.InvokeTemplate(nameof(OverrideMethod));
        }
        else
        {
            meta.InvokeTemplate(nameof(OverrideMethod), this);
        }

        throw new Exception();
    }
}

class DerivedAspect : Aspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("overridden virtual method");

        return meta.Proceed();
    }
}

// <target>
class TargetCode
{
    [Aspect]
    private async Task Method1(bool condition)
    {
        await Task.Yield();
    }

    [DerivedAspect]
    private async Task Method2(bool condition)
    {
        await Task.Yield();
    }
}