using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssignAwaitTask
{
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            _ = await meta.ProceedAsync();
            
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