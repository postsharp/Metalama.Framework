#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
// @LangVersion(12)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.PrimaryConstructor_Runtime;

public class TheAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(M));
    }

    [Template]
    public void M()
    {
        Console.WriteLine("Aspect");
        meta.Proceed();
    }
}

public class B(int x)
{
    public int X { get; set; } = x;
}

// <target>
public class C(int x) : B(x)
{
    [TheAspect]
    public void M()
    {
        _ = new C(42);
    }
}

#endif