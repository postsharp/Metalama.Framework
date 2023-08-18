using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.DirectCall;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");
        CalledTemplateInstance();
        CalledTemplateStatic();
        new AnotherClass().CalledTemplateInstance();
        AnotherClass.CalledTemplateStatic();
        return default;
    }

    [Template]
    private void CalledTemplateInstance()
    {
        Console.WriteLine("called template instance aspect");
    }

    [Template]
    private static void CalledTemplateStatic()
    {
        Console.WriteLine("called template static aspect");
    }
}

[TemplateProvider]
class AnotherClass
{
    [Template]
    public void CalledTemplateInstance()
    {
        Console.WriteLine("called template instance another");
    }

    [Template]
    public static void CalledTemplateStatic()
    {
        Console.WriteLine("called template static another");
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