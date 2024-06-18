using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.IntroduceMethod;

#pragma warning disable CS0219

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        builder.IntroduceMethod( nameof(Method), args: new { T = builder.Target, x = 42 } );
    }

    [Template]
    private T? Method<[CompileTime] T>( [CompileTime] int x, T p, T[] p2, List<T> p3 ) where T : class
    {
        return default;
    }
}

// <target>
[Aspect]
public class Target { }