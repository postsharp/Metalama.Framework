using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnVoidResultAndNullAwait
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
                return null;
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