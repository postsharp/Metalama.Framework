using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseCompileTimeInRunTime;

internal class C
{
    private void M()
    {
        _ = meta.Target.Compilation;
    }
}