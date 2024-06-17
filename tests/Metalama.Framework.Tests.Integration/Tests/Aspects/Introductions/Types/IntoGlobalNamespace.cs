#if TEST_OPTIONS
// @OutputAllSyntaxTrees
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntoGlobalNamespace
{

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceClass(builder.Target.Compilation.GlobalNamespace, "TestType");
            builder.Advice.IntroduceClass(builder.Target.Compilation.GlobalNamespace, "TestType", buildType: t => t.AddTypeParameter("T"));
        }
    }

    // <target>
    [IntroductionAttribute]
    public class TargetType { }
}