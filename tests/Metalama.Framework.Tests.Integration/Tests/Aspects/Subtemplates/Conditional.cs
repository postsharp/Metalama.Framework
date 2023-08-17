using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

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