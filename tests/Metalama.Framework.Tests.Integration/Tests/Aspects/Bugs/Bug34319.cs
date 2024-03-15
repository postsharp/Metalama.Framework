using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug34319;

public class IntroduceParametersAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        for (int i = 0; i < 3; i++)
        {
            builder.Advice.IntroduceParameter(builder.Target.Constructors.Single(), $"p{i}", typeof(int), TypedConstant.Create(0));
        }
    }
}

// <target>
[IntroduceParameters]
class TargetWithoutConstructor
{
}

// <target>
[IntroduceParameters]
class TargetWithConstructor
{
    public TargetWithConstructor(string s)
    {
    }
}