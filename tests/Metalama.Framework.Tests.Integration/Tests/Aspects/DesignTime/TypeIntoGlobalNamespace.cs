#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.TypeIntoGlobalNamespace
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceClass(builder.Target.Compilation.GlobalNamespace, "TestType");
        }
    }

    // <target>
    [Introduction]
    public class TargetType { }
}