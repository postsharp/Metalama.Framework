using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Dynamic.DiscardAssignVoid
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
        void Method(int a)
        {
            Console.WriteLine("Hello, world.");
        }
        
    }
}