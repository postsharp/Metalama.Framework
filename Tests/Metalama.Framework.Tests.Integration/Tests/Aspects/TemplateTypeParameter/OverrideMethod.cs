using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.OverrideMethod;

public class Aspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        base.BuildAspect( builder );

        builder.Advice.Override( builder.Target, nameof(Method), args: new { T = builder.Target.ReturnType } );
    }

    [Template]
    private T Method<[CompileTime] T>() where T : IConvertible
    {
        var x = default(T);
        var t = (T)meta.Proceed();
        var z = t.ToBoolean( null );

        return t;
    }
}

// <target>
public class Target
{
    [Aspect]
    public int M() => 5;
}