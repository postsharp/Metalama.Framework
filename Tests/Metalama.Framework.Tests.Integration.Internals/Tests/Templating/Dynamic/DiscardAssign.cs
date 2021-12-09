using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssign
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            _ = meta.Proceed();
            
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