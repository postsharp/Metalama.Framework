using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CallingTemplates.DynamicCall;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");
        meta.InvokeTemplate(nameof(CalledTemplateSimple));
        meta.InvokeTemplate(new TemplateInvocation(nameof(CalledTemplateInvocation), null));
        var templateProvider = new Templates();
        meta.InvokeTemplate(nameof(Templates.CalledTemplateSimple), templateProvider);
        meta.InvokeTemplate(new TemplateInvocation(nameof(Templates.CalledTemplateInvocation), templateProvider));
        return default;
    }

    [Template]
    private void CalledTemplateSimple()
    {
        Console.WriteLine("called template simple aspect");
    }

    [Template]
    private void CalledTemplateInvocation()
    {
        Console.WriteLine("called template invocation aspect");
    }
}

[CompileTime]
class Templates : ITemplateProvider
{
    [Template]
    public void CalledTemplateSimple()
    {
        Console.WriteLine("called template simple provider");
    }

    [Template]
    public void CalledTemplateInvocation()
    {
        Console.WriteLine("called template invocation provider");
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