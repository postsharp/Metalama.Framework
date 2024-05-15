﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsField_SelfReferencing;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "IntroducedNestedType", TypeKind.Class, buildType: t => { t.Accessibility = Code.Accessibility.Public; });
        var existingNested = builder.Target.NestedTypes.Single();

        builder.Advice.IntroduceField(
            result.Declaration,
            nameof(FieldTemplate),
            buildField: b =>
            {
                b.Name = "Field";
                b.Type = result.Declaration;
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