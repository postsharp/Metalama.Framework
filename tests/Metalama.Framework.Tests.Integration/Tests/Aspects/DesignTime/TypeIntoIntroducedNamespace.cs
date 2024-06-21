#if TEST_OPTIONS
// @DesignTime
#endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.TypeIntoIntroducedNamespace;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var @namespace = builder.With( builder.Target.ContainingNamespace ).WithChildNamespace( "IntroducedNamespace" );
        builder.IntroduceClass( "TestType" );
    }
}

// <target>
[Introduction]
public class TargetType { }