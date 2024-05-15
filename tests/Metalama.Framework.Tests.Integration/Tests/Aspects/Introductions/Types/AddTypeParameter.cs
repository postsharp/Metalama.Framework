using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AddTypeParameter;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceClass( builder.Target, "TestNestedType", TypeKind.Class, buildType: b => { b.AddTypeParameter( "T" ); } );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }