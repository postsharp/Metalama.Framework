using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual;

#pragma warning disable CS1998 // Async method lacks 'await' operators

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("virtual method");

        return meta.Proceed();
    }

    public async override Task<dynamic?> OverrideAsyncMethod()
    {
        Console.WriteLine("normal template");

        OverrideMethod();

        OverrideMethodAspect overrideMethodAspect = this;
        overrideMethodAspect.OverrideMethod();

        ((OverrideMethodAspect)this).OverrideMethod();

        throw new Exception();
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private async Task Method()
    {
    }
}