using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntroduceField;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.Advice.IntroduceClass( builder.Target, "TestNestedType", TypeKind.Class );

        builder.Advice.IntroduceField( result.Declaration, nameof(Field) );
    }

    [Template]
    public int Field;
}

// <target>
[IntroductionAttribute]
public class TargetType { }