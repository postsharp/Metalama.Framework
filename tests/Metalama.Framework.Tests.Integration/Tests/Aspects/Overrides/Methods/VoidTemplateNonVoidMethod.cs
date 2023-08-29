using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.VoidTemplateNonVoidMethod;

public class OverrideAttribute : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(OverrideMethod));
    }

    [Template]
    public void OverrideMethod(dynamic arg)
    {
        if (arg == null)
        {
            Console.WriteLine("error");
            meta.Return(default);
        }
        meta.Return(meta.Proceed());
    }
}

// <target>
internal class TargetClass
{
    [Override]
    void VoidMethod(object arg)
    {
        Console.WriteLine("void method");
    }

    [Override]
    int IntMethod(object arg)
    {
        Console.WriteLine("int method");
        return 42;
    }
}
