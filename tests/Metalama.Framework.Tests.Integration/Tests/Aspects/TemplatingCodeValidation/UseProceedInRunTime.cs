namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseProceedInRunTime;

using Metalama.Framework.Aspects;

internal class C
{
    private void M()
    {
        meta.Proceed();
    }
}