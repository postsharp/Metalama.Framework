#pragma warning disable CS8600, CS8603
using Metalama.TestFramework;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnDefaultAwait
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            try
            {
                dynamic result = await meta.Proceed();
                return result;
            }
            catch
            {
                return default;
            }
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return 42 / a;
        }
    }
}