using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.TemplateProvider_Struct;

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

        new Templates().CalledTemplate();
        new Templates().CalledTemplate();

        return meta.Proceed();
    }
}

struct Templates : ITemplateProvider
{
    [Template]
    public void CalledTemplate()
    {
        Console.WriteLine($"called template");
    }
}