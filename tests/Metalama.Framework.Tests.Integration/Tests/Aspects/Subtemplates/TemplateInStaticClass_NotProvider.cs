using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.TemplateInStaticClass_NotProvider;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");
        StaticClass.StaticTemplate(1, 2);
        return default;
    }
}

[RunTimeOrCompileTime]
static class StaticClass
{
    [Template]
    public static void StaticTemplate(int i, [CompileTime] int j)
    {
        Console.WriteLine($"static template i={i}, j={j}");
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