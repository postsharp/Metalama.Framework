#if TEST_OPTIONS
//@Include(_PullStrategy.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection;
using Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.PullAndInitializer;

[assembly: AspectOrder(typeof(OverrideAspect), typeof(MyAspect))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.PullAndInitializer;

public class OverrideAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach(var field in builder.Target.Fields)
        {
            builder.Advice.AddInitializer(builder.Target, nameof(Template), InitializerKind.BeforeInstanceConstructor);
        }
    }

    [Template]
    public void Template()
    {
        Console.WriteLine($"{meta.Target} initialized.");
    }
}

// <target>
[MyAspect]
[OverrideAspect]
public class TestClass
{
    public TestClass() { }
}