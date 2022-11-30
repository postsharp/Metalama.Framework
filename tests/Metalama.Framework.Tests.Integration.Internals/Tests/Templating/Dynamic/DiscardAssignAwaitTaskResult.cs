using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssignAwaitTaskResult
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
        async Task<int> Method(int a)
        {
            await Task.Yield();
            return a;
        }
        
    }
}