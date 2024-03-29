using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntroduceField;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "TestNestedType");

        builder.Advice.IntroduceMethod(result.Declaration.ForCompilation(builder.Advice.MutableCompilation), nameof(MethodTemplate) );
    }

    [Template]
    public void MethodTemplate()
    {
    }
}

// <target>
[IntroductionAttribute]
public class TargetType
{
}