#if TEST_OPTIONS
//@Include(_PullStrategy.cs)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection;
using Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.PullAndOverride;

[assembly: AspectOrder(typeof(OverrideAspect), typeof(MyAspect))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection.PullAndOverride;

public class OverrideAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach(var field in builder.Target.Fields)
        {
            builder.Advice.Override(field, nameof(Template));
        }
    }

    [Template]
    public dynamic? Template
    {
        get
        {
            Console.WriteLine("Aspect code");
            return meta.Proceed();
        }

        set
        {
            Console.WriteLine("Aspect code");
            meta.Proceed();
        }
    }
}

// <target>
[MyAspect]
[OverrideAspect]
public class TestClass
{
    public TestClass() { }
}