using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.StaticConstructor;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceParameter( builder.Target.StaticConstructor!, "p", typeof(int), TypedConstant.Create( 15 ) );
    }
}

// <target>
[MyAspect]
public class C
{
    static C() { }
}