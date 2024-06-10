#if TEST_OPTIONS
// @OutputAllSyntaxTrees
# endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Namespaces.Conflict_WithSource
{

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var n = builder.Advice.IntroduceNamespace(builder.Target.ContainingNamespace, "TestNamespace");
            builder.Advice.IntroduceClass(n.Declaration, "TestNestedType");
        }
    }

    // <target>
    [IntroductionAttribute]
    public class TargetType
    {
    }

    namespace TestNamespace
    {
        public class Placeholder { }
    }
}