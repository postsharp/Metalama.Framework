using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.ImplicitConstructor;

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
public class C { }