using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsMethodParameter;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "TestNestedType");

        builder.Advice.IntroduceMethod(builder.Target, nameof(MethodTemplate), buildMethod: b => b.AddParameter("p", result.Declaration));
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