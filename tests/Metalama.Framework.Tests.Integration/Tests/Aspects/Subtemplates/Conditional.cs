using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Conditional;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var templates = Templates.Create();

        templates?.CalledTemplate();

        return default;
    }
}

[TemplateProvider]
class Templates
{
    [CompileTime]
    public static Templates? Create() => new();

    [Template]
    public void CalledTemplate()
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