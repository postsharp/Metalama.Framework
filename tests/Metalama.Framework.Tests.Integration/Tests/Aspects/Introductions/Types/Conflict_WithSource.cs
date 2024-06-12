using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.Conflict_WithSource;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceClass( builder.Target, "TestNestedType" );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType 
{
    public class TestNestedType { }
}