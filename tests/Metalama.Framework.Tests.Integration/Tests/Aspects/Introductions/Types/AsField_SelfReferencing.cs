﻿using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsField_SelfReferencing;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.IntroduceClass(
            "IntroducedNestedType",
            buildType: t => { t.Accessibility = Code.Accessibility.Public; } );

        var existingNested = builder.Target.Types.Single();

        result.IntroduceField(
            nameof(FieldTemplate),
            buildField: b =>
            {
                b.Name = "Field";
                b.Type = result.Declaration;
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