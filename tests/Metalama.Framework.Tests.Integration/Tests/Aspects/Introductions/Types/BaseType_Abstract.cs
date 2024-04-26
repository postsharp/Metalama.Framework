using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_Abstract;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceType(builder.Target, "TestNestedType", TypeKind.Class, buildType: t => { t.BaseType = builder.Target; });
    }
}

// <target>
[IntroductionAttribute]
public abstract class TargetType
{
    public abstract int Property { get; set; }

    public abstract void Method();
}