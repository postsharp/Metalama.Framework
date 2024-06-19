#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntoFileScopedNamespace;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.With( builder.Target.ContainingNamespace ).IntroduceClass( "TestType" );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }