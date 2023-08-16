using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.ExtensionMethodTemplate;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");
        this.Extension();
        return default;
    }
}

[RunTimeOrCompileTime]
static class StaticClass
{
    [Template]
    public static void Extension([CompileTime] this Aspect aspect)
    {
        Console.WriteLine("extension template");
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