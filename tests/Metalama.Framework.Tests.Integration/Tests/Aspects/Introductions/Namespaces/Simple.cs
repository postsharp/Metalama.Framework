#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.Simple;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var @namespace = builder.With( builder.Target.Compilation.GlobalNamespace ).IntroduceNamespace( "Implementation" );
        var @class = @namespace.IntroduceClass( "Test" );

        builder.IntroduceField( "Field", @class.Declaration );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }