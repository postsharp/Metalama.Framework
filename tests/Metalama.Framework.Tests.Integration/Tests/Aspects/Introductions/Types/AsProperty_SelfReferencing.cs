using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsProperty_SelfReferencing;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "IntroducedNestedType", TypeKind.Class, buildType: t => { t.Accessibility = Accessibility.Public; });
        var existingNested = builder.Target.NestedTypes.Single();

        builder.Advice.IntroduceProperty(
            result.Declaration,
            nameof(PropertyTemplate),
            buildProperty: b =>
            {
                b.Name = "Property";
                b.Type = result.Declaration;
            });
    }

    [Template]
    public object? PropertyTemplate { get; set; }
}

// <target>
[IntroductionAttribute]
public class TargetType
{
    public class ExistingNestedType
    {
    }
}