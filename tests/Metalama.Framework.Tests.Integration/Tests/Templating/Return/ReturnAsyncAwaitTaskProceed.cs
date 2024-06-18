using System;
using Metalama.Framework.Aspects;
using System.Threading.Tasks;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnAsyncAwaitTaskProceed
{
    [CompileTime]
    internal class Aspect
    {
        [TestTemplate]
        private async Task<dynamic?> Template()
        {
            return await meta.ProceedAsync();
        }
    }

    internal class TargetCode
    {
        // <target>
        private async Task Method( int a, int b )
        {
            await Task.Yield();
            Console.WriteLine( a / b );
        }
    }
}