#if TEST_OPTIONS
// @OutputAllSyntaxTrees
# endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.Conflict_WithSource
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var ns = builder.With( builder.Target.ContainingNamespace ).WithChildNamespace( "TestNamespace" );
            ns.IntroduceClass( "TestNestedType" );
        }
    }

    // <target>
    [IntroductionAttribute]
    public class TargetType { }

    namespace TestNamespace
    {
        public class Placeholder { }
    }
}