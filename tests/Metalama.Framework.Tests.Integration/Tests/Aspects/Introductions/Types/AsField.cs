﻿using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsField;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.IntroduceClass(
            "IntroducedNestedType",
            buildType: t => { t.Accessibility = Code.Accessibility.Public; } );

        var existingNested = builder.Target.Types.Single();

        builder.IntroduceField(
            nameof(FieldTemplate),
            buildField: b =>
            {
                b.Name = "FieldWithIntroduced";
                b.Type = result.Declaration;
            } );

        builder.IntroduceField(
            nameof(FieldTemplate),
            buildField: b =>
            {
                b.Name = "FieldWithExisting";
                b.Type = existingNested;
            } );
    }

    [Template]
    public object? FieldTemplate;
}

// <target>
[IntroductionAttribute]
public class TargetType
{
    public class ExistingNestedType { }
}