#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.TypeWithMembersIntoNamespace
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var type = builder.Advice.IntroduceClass(builder.Target.ContainingNamespace, "TestType");
            type.IntroduceField("TestField", typeof(int));
        }
    }

    // <target>
    namespace TargetNamespace
    {
        [Introduction]
        public class TargetType { }
    }
}