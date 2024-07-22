#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceField;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.IntroduceField(nameof(Field));
    }

    [Template]
    public int Field;
}

// <target>
[Introduction]
internal partial class TargetClass
{
}