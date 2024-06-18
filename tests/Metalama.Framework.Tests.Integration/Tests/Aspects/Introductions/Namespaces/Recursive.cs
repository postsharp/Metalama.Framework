#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.Recursive;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var outerNamespace = builder.With( builder.Target.Compilation ).IntroduceNamespace( "Outer" );
        var middleNamespace = outerNamespace.IntroduceNamespace( "Middle" );
        var innerNamespace = middleNamespace.IntroduceNamespace( "Inner" );
        var @class = innerNamespace.IntroduceClass( "Test" );

        builder.IntroduceField( "Field", @class.Declaration );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }