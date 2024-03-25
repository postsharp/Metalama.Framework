#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.PrimaryConstructor_Pull;

public class MyAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceParameter(builder.Target.PrimaryConstructor, "p", typeof(int), TypedConstant.Create(15), (p, c) => PullAction.UseExpression(TypedConstant.Create(51)));
    }
}

// <target>
[MyAspect]
public class A(int x)
{
    public A(short x) : this((int)x)
    {
    }

    public int X { get; set; } = x;
}

#endif