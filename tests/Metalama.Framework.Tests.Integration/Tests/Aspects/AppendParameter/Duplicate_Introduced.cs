using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AppendParameter.Duplicate_Introduced;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach (var constructor in builder.Target.Constructors)
        {
            builder.Advice.IntroduceParameter(constructor, "p", typeof(int), TypedConstant.Create(13));
            builder.Advice.IntroduceParameter(constructor, "p", typeof(int), TypedConstant.Create(42));
        }
    }
}

// <target>
[MyAspect]
public class C 
{
    public C()
    {
    }
}