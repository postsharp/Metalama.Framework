#pragma warning disable CS8600, CS8603
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.UnsupportedSyntax.GotoNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = meta.Proceed();

            if (result != null) goto end;

            return default;

        end:
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}