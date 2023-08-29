using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.Parameters_Named;

class Aspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(Template), args: new { b = 1, a = 2 });
        builder.Advice.Override(builder.Target, nameof(Template));
        builder.Advice.Override(builder.Target, nameof(Template), args: new { b = 2 });
    }

    [Template]
    void Template([CompileTime] int a = -1, [CompileTime] int b = -2)
    {
        Console.WriteLine($"template a={a} b={b}");
        meta.Proceed();
    }
}

class TargetCode
{
    // <target>
    [Aspect]
    void M()
    {
    }
}