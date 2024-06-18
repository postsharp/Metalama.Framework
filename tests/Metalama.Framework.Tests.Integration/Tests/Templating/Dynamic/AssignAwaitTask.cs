using System;
using System.Threading.Tasks;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.AssignAwaitTask
{
    internal class Aspect
    {
        [TestTemplate]
        private async Task<dynamic?> Template()
        {
            var x = TypeFactory.GetType( SpecialType.Int32 ).DefaultValue();

            x = await meta.ProceedAsync();
            x += await meta.ProceedAsync();
            x *= await meta.ProceedAsync();

            return default;
        }
    }

    internal class TargetCode
    {
        private async Task Method( int a )
        {
            await Task.Yield();
            Console.WriteLine( "Hello, world." );
        }
    }
}