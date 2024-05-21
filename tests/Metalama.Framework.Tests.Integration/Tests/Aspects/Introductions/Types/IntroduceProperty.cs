using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntroduceProperty;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.Advice.IntroduceClass( builder.Target, "TestNestedType", TypeKind.Class );

        result.IntroduceProperty( nameof(Property) );
    }

    [Template]
    public int Property { get; set; }
}

// <target>
[IntroductionAttribute]
public class TargetType { }