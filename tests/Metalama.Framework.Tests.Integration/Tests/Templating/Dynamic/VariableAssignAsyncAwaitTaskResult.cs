using System;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.VariableAssignAsyncAwaitTaskResult
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            var result = await meta.ProceedAsync();
            return result;
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