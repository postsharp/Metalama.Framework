using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.AspectParameter_Error;

internal class Aspect : OverrideMethodAspect
{
    public Aspect(int i)
    {
        I = i;
    }

    public int I { get; }

    public override dynamic? OverrideMethod()
    {
        var aspect = this;
        AnotherClass.CalledTemplate(aspect);
        AnotherClass.CalledTemplate(this);
        return default;
    }
}

[TemplateProvider]
static class AnotherClass
{
    [Template]
    public static void CalledTemplate(Aspect aspect)
    {
        Console.WriteLine($"called template i={aspect.I}");
    }
}

class TargetCode
{
    // <target>
    [Aspect(42)]
    void Method()
    {
    }
}