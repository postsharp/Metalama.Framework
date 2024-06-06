// @OutputAllSyntaxTrees

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.IntoExisting;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var @namespace = builder.Advice.IntroduceNamespace( builder.Target.ContainingNamespace, "Implementation" );
        var @class = @namespace.IntroduceClass("TestClass");

        builder.IntroduceField( "Field", @class.Declaration );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }