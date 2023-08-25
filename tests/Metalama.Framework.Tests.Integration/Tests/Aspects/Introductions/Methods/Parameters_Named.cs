using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Parameters_Named;

class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceMethod(builder.Target, nameof(Template), buildMethod: m => m.Name = "M1", args: new { b = 1, a = 2 });
        builder.Advice.IntroduceMethod(builder.Target, nameof(Template), buildMethod: m => m.Name = "M2");
        builder.Advice.IntroduceMethod(builder.Target, nameof(Template), buildMethod: m => m.Name = "M3", args: new { b = 2 });
    }

    [Template]
    void Template([CompileTime] int a = -1, [CompileTime] int b = -2)
    {
        Console.WriteLine($"template a={a} b={b}");
    }
}

// <target>
[Aspect]
class TargetCode
{
}