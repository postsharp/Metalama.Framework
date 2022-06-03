namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseProceedInTemplate;

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;


internal class RetryAttribute : OverrideMethodAspect
{
    // Template that overrides the methods to which the aspect is applied.
    public override dynamic? OverrideMethod()
    {
        return meta.Proceed();
    }

    [Template]
    public void Method()
    {
        meta.Proceed();
    }
}

