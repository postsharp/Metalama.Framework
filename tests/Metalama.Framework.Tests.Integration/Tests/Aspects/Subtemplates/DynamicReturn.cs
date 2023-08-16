using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.DynamicReturn;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");
        CalledTemplate();
        throw new Exception();
    }

    [Template]
    dynamic? CalledTemplate()
    {
        Console.WriteLine("called template");
        return meta.Proceed();
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private void VoidMethod()
    {
    }

    [Aspect]
    private int IntMethod()
    {
        return 42;
    }
}