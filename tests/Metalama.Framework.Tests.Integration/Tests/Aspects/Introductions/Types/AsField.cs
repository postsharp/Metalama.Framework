using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsField;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "IntroducedNestedType", TypeKind.Class, buildType: t => { t.Accessibility = Accessibility.Public; });
        var existingNested = builder.Target.NestedTypes.Single();

        builder.Advice.IntroduceField(
            builder.Target.ForCompilation(builder.Advice.MutableCompilation), 
            nameof(FieldTemplate), 
            buildField: b => 
            {
                b.Name = "FieldWithIntroduced";
                b.Type = result.Declaration;
            });

        builder.Advice.IntroduceField(
            builder.Target.ForCompilation(builder.Advice.MutableCompilation),
            nameof(FieldTemplate),
            buildField: b =>
            {
                b.Name = "FieldWithExisting";
                b.Type = existingNested;
            });
    }

    [Template]
    public object? FieldTemplate;
}

// <target>
[IntroductionAttribute]
public class TargetType
{
    public class ExistingNestedType
    {
    }
}