using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.Recursive;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introduced = builder.IntroduceClass( "Test" );
        var inner = introduced.IntroduceClass( "InnerTest" );
        inner.IntroduceClass( "InnerInnerTest" );
    }

    [Template]
    public void Method() { }
}

// <target>
[IntroductionAttribute]
public class TargetType { }