#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.PrimaryConstructor;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            Debugger.Break();
            builder.Advice.IntroduceParameter( constructor, "p", typeof(int), TypedConstant.Create( 15 ) );
        }
    }
}

public class A(int x)
{
    public int X { get; set; } = x;
}

// <target>
[MyAspect]
public class C(int x) : A(42)
{
    public int Y { get; } = x;
}

#endif