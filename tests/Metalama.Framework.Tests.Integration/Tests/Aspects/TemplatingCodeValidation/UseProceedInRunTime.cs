namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseProceedInRunTime;

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

internal class C
{
    private void M()
    {
        meta.Proceed();
    }
}