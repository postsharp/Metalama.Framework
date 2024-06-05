using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsMethodParameter;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.Advice.IntroduceClass(
            builder.Target,
            "IntroducedNestedType",
            buildType: t => { t.Accessibility = Code.Accessibility.Public; } );

        var existingNested = builder.Target.Types.Single();

        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(MethodTemplate),
            buildMethod: b =>
            {
                b.Name = "MethodWithIntroduced";
                b.AddParameter( "p", result.Declaration );
            } );

        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(MethodTemplate),
            buildMethod: b =>
            {
                b.Name = "MethodWithExisting";
                b.AddParameter( "p", existingNested );
            } );
    }

    [Template]
    public void MethodTemplate() { }
}

// <target>
[IntroductionAttribute]
public class TargetType
{
    public class ExistingNestedType { }
}