using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.PrimaryConstructor_TargetAll;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            builder.Advice.IntroduceParameter(constructor, "p", typeof(int), TypedConstant.Create(15));
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