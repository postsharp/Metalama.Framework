using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.AssignAwaitTaskResult
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
        async Task<int> Method(int a)
        {
            await Task.Yield();
            return a;
        }
        
    }
}