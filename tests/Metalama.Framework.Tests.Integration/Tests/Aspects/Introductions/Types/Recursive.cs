using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.Recursive;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introduced = builder.IntroduceClass( "Test", TypeKind.Class );
        var inner = introduced.IntroduceClass( "InnerTest", TypeKind.Class );
        inner.IntroduceClass( "InnerInnerTest", TypeKind.Class );
    }

    [Template]
    public void Method() { }
}

// <target>
[IntroductionAttribute]
public class TargetType { }