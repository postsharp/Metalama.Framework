#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
// @LangVersion(12)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.PrimaryConstructor_TargetNonPrimary;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            if (constructor is { Parameters.Count: 2 })
            {
                builder.Advice.IntroduceParameter(constructor, "p", typeof(int), TypedConstant.Create(15));
            }
        }
    }
}

public class A(int x)
{

}

// <target>
[MyAspect]
public class C(int x) : A(42)
{
    public int X { get; } = x;

    public C(int x, int y) : this(x)
    {
    }
}

#endif