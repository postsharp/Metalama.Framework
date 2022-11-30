using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnAsyncAwaitTaskProceed
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            return await meta.ProceedAsync();
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