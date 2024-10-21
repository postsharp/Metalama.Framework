using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.AspectTests.Tests.Aspects.Introductions.Types.Conflict_WithIntroduced;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.IntroduceClass( "TestNestedType" );
        builder.IntroduceClass( "TestNestedType" );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }