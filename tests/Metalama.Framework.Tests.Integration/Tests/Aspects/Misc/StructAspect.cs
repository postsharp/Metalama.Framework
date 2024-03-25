using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.StructAspect;

struct Aspect : IAspect<IMethod>
{
    public void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(builder.Target, nameof(OverrideMethod));
    }

    public void BuildEligibility(IEligibilityBuilder<IMethod> builder)
    {
    }

    [Template]
    dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");

        return meta.Proceed();
    }
}