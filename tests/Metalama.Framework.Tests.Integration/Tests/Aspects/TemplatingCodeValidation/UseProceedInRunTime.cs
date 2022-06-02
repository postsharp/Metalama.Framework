namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseProceedInRunTime;

using Metalama.Framework.Aspects;


class C
{
    void M()
    {
        meta.Proceed();
    }
}