using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnAsyncAwaitTaskResultProceed
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
        async Task<int> Method(int a, int b)
        {
            await Task.Yield();
            Console.WriteLine(a / b);
            return 1;
        }
    }
}