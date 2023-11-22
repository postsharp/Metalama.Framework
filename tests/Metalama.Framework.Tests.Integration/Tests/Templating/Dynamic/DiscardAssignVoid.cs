using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssignVoid
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