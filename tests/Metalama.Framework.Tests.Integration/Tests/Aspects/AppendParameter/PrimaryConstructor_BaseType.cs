#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.PrimaryConstructor_BaseType;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            builder.Advice.IntroduceParameter( constructor, "p", typeof(int), TypedConstant.Create( 15 ) );
        }
    }
}

// <target>
[MyAspect]
public class A(int x)
{
    public int X { get; set; } = x;
}

// <target>
public class C(int x) : A(42)
{
    public int Y { get; } = x;
}

#endif