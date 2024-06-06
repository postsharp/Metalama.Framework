#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.TypeIntoIntroducedNamespace;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var @namespace = builder.Advice.IntroduceNamespace(builder.Target.ContainingNamespace, "IntroducedNamespace");
        builder.Advice.IntroduceClass(@namespace.Declaration, "TestType");
    }
}

// <target>
[Introduction]
public class TargetType { }