using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsMethodParameter;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var result = builder.Advice.IntroduceType(builder.Target, "IntroducedNestedType", TypeKind.Class, buildType: t => { t.Accessibility = Accessibility.Public; });

        builder.Advice.IntroduceMethod(
            builder.Target.ForCompilation(builder.Advice.MutableCompilation), 
            nameof(MethodTemplate), 
            buildMethod: b => 
            {
                b.Name = "MethodWithIntroduced";
                b.AddParameter("p", result.Declaration);
            });

        builder.Advice.IntroduceMethod(
            builder.Target.ForCompilation(builder.Advice.MutableCompilation),
            nameof(MethodTemplate),
            buildMethod: b =>
            {
                b.Name = "MethodWithExisting";
                b.AddParameter("p", builder.Target.NestedTypes.Single());
            });
    }

    [Template]
    public void MethodTemplate()
    {
    }
}

// <target>
[IntroductionAttribute]
public class TargetType
{
    public class ExistingNestedType
    {
    }
}