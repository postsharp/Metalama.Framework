using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.IntroduceIndexer;

#pragma warning disable CS0219

public class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.IntroduceIndexer(builder.Target, typeof(int), nameof(GetTemplate), nameof(SetTemplate), args: new { T = builder.Target, x = 42 });
        builder.Advice.IntroduceIndexer(builder.Target, typeof(string), nameof(GetTemplate), null, args: new { T = builder.Target, x = 42 });
        builder.Advice.IntroduceIndexer(builder.Target, typeof(object), null, nameof(SetTemplate), args: new { T = builder.Target, x = 42 });
    }

    [Template]
    private T? GetTemplate<[CompileTime] T>( [CompileTime] int x, dynamic index ) where T : class
    {
        return default;
    }

    [Template]
    private void SetTemplate<[CompileTime] T>([CompileTime] int x, dynamic index, T p) where T : class
    {
    }
}

// <target>
[Aspect]
public class Target { }