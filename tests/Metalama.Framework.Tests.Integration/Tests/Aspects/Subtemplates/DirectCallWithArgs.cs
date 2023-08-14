using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.DirectCallWithArgs;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");
        CalledTemplate(42);
        return default;
    }

    [Template]
    private void CalledTemplate([CompileTime] int i)
    {
        Console.WriteLine($"called template i={i}");
    }
}

internal class TargetCode
{
    // <target>
    [Aspect]
    private void Method()
    {
    }
}