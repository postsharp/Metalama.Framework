using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseProceedInCompileTimeOnlyNonTemplate.cs;

[CompileTime]
internal class C
{
    private void M()
    {
        meta.Proceed();
    }
}