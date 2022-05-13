using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateParameters.OverrideMethod;

public class Aspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        base.BuildAspect( builder );

        builder.Advice.Override( builder.Target, nameof(Method), args: new { a = 5, t = builder.Target.ReturnType } );
    }

    [Template]
    private void Method( [CompileTime] int a, IType t )
    {
        Console.WriteLine( a );
        Console.WriteLine( t.ToDisplayString() );
    }
}

// <target>
public class Target
{
    [Aspect]
    public void M() { }
}