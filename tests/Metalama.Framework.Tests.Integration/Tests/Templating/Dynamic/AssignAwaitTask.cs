using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.AssignAwaitTask
{
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            dynamic? x = TypeFactory.GetType(SpecialType.Int32).DefaultValue();

            x = await meta.ProceedAsync();
            x += await meta.ProceedAsync();
            x *= await meta.ProceedAsync();

            return default;
        }
    }

    class TargetCode
    {
        async Task Method(int a)
        {
            await Task.Yield();
            Console.WriteLine("Hello, world.");
        }
        
    }
}