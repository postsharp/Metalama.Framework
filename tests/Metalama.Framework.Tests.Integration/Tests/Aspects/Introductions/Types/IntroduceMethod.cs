using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntroduceMethod;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.IntroduceClass( "TestNestedType" );

        result.IntroduceMethod( nameof(Method) );
    }

    [Template]
    public void Method() { }
}

// <target>
[IntroductionAttribute]
public class TargetType { }