using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.AspectParameter;

internal class Aspect : OverrideMethodAspect
{
    public Aspect(int i)
    {
        I = i;
    }

    public int I { get; }

    public override dynamic? OverrideMethod()
    {
        var aspect = meta.RunTime(new Aspect(this.I));
        AnotherClass.RunTimeAspect(aspect);
        AnotherClass.CompileTimeAspect(this);
        return default;
    }
}

[TemplateProvider]
static class AnotherClass
{
    [Template]
    public static void RunTimeAspect(Aspect aspect)
    {
        Console.WriteLine($"run-time i={aspect.I}");
    }

    [Template]
    public static void CompileTimeAspect([CompileTime] Aspect aspect)
    {
        Console.WriteLine($"compile-time i={aspect.I}");
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