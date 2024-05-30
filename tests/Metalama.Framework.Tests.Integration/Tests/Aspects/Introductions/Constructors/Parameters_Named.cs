using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Constructors.Parameters_Named;

class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceConstructor(builder.Target, nameof(Template), args: new { b = 1, a = 2 });
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