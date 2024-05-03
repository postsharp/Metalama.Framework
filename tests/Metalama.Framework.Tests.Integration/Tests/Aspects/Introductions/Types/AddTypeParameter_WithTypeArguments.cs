#if TESTOPTIONS
// @Skipped(constructed generics are not supported)
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AddTypeParameter_WithTypeArguments;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "TestNestedType", TypeKind.Class, buildType: b => { b.AddTypeParameter("T"); b.Accessibility = Accessibility.Public; });

        builder.Advice.IntroduceMethod(builder.Target, nameof(Template), buildMethod: b => { b.AddParameter("p", result.Declaration.WithTypeArguments(TypeFactory.GetType(SpecialType.Object))); });
    }

    [Template]
    public void Template()
    {
    }
}

// <target>
[IntroductionAttribute]
public class TargetType
{
}