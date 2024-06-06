using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsProperty;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceClass(builder.Target, "IntroducedNestedType", buildType: t => { t.Accessibility = Code.Accessibility.Public; });
        var existingNested = builder.Target.Types.Single();

        builder.Advice.IntroduceProperty(
            builder.Target, 
            nameof(PropertyTemplate),
            buildProperty: b => 
            {
                b.Name = "PropertyWithIntroduced";
                b.Type = result.Declaration;
            });

        builder.Advice.IntroduceProperty(
            builder.Target,
            nameof(PropertyTemplate),
            buildProperty: b =>
            {
                b.Name = "PropertyWithExisting";
                b.Type = existingNested;
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