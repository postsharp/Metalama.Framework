using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntoFileScopedNamespace;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceClass( builder.Target.ContainingNamespace, "TestType", TypeKind.Class );
    }
}

[IntroductionAttribute]
public class TargetType { }