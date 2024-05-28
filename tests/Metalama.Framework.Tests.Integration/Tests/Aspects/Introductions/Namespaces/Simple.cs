using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.Simple;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var @namespace = builder.Advice.IntroduceNamespace( builder.Target.ContainingNamespace, "Implementations" );
        var @class = @namespace.IntroduceClass("Test");

        builder.IntroduceField("Field", @class.Declaration);
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }