using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Proceed;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        throw new NotImplementedException();
    }

    public override async Task<dynamic?> OverrideAsyncMethod()
    {
        await Task.Yield();
        Console.WriteLine("regular template");

        if (meta.Target.Parameters[0].Value)
        {
            StaticClass.StaticTemplate(1);
        }
        else
        {
            meta.InvokeTemplate(nameof(StaticClass.StaticTemplate), typeof(StaticClass), new { i = 2 });
        }

        throw new Exception();
    }
}

[TemplateProvider]
static class StaticClass
{
    [Template]
    public static void StaticTemplate([CompileTime] int i)
    {
        Console.WriteLine($"static template i={i}");
        meta.Return(meta.Proceed());
    }
}

internal class TargetCode
{
    // <target>
    [Aspect]
    private async Task Method(bool condition)
    {
        await Task.Yield();
    }
}