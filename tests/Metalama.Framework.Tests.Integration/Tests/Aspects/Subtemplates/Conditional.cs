using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Conditional;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        this?.CalledTemplate();

        return default;
    }

    [Template]
    private void CalledTemplate()
    {
        Console.WriteLine("called template");
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private void Method()
    {
    }
}