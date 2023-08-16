using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Subexpression;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var x = CalledTemplate();
        Console.WriteLine(CalledTemplate());

        for (CalledTemplate(); CalledTemplate(); CalledTemplate()) ;

        return CalledTemplate();
    }

    [Template]
    dynamic? CalledTemplate()
    {
        return meta.Proceed();
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