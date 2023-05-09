using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32463;

#pragma warning disable CS0169

public class BeforeCtorAttribute : TypeAspect
{
    [Introduce]
    private int f;

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.AddInitializer(builder.Target, nameof(this.BeforeInstanceConstructor), InitializerKind.BeforeInstanceConstructor);
    }

    [Template]
    private void BeforeInstanceConstructor()
    {
        Console.WriteLine("before ctor");
    }
}

// <target>
[BeforeCtor]
struct S { }
