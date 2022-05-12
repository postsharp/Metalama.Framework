using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateParameters.IntroduceMethod;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        builder.Advice.IntroduceMethod( builder.Target, nameof(Method), args: new { a = 5 } );
    }

    [Template]
    private void Method( [CompileTime] int a, int b )
    {
        Console.WriteLine( a );
        Console.WriteLine( b );
    }
}

// <target>
[Aspect]
public class Target { }