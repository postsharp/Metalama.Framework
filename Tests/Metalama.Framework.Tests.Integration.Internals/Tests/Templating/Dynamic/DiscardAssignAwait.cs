using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssignAwait
{
    class Aspect
    {
        [TestTemplate]
        async Task<dynamic?> Template()
        {
            _ = await meta.Proceed();
            
            return default;
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
        
    }
}