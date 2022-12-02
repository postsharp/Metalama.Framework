// @Skipped(#30249)

using System;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.VariableAssignAsyncTask
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            var result = meta.ProceedAsync();
            return await result;
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