using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.Recursive;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "Test", TypeKind.Class);
        var innerResult = builder.Advice.IntroduceType(result.Declaration, "InnerTest", TypeKind.Class);
        builder.Advice.IntroduceType(innerResult.Declaration, "InnerInnerTest", TypeKind.Class);
    }

    [Template]
    public void Method()
    {
    }
}

// <target>
[IntroductionAttribute]
public class TargetType
{
}