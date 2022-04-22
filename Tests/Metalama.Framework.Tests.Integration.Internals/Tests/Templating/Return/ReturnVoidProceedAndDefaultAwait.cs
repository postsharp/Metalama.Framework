using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnVoidProceedAndDefaultAwait
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            try
            {
                return await meta.Proceed();
            }
            catch
            {
                return default;
            }
        }
    }

    class TargetCode
    {
        async Task Method(int a, int b)
        {
            await Task.Yield();
            Console.WriteLine(a / b);
        }
    }
}