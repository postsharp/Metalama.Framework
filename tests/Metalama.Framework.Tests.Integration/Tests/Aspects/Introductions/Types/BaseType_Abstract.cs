using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_Abstract;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "TestNestedType", TypeKind.Class, buildType: t => { t.BaseType = builder.Target; });

        builder.Advice.IntroduceProperty(builder.Target, nameof(Property), whenExists: OverrideStrategy.Override);
        builder.Advice.IntroduceMethod(builder.Target, nameof(Method), whenExists: OverrideStrategy.Override);
    }

    [Template]
    public int Property { get; set; }

    [Template]
    public void Method()
    {
    }
}

// <target>
[IntroductionAttribute]
public abstract class TargetType
{
    public abstract int Property { get; set; }

    public abstract void Method();
}