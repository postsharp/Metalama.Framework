using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.ProceedInOldSwitchRunTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int i = 1;

            switch (i)
            {
                case 0:
                    Console.WriteLine("0");
                    break;
                case 1:
                    var x = meta.Proceed();
                    break;
            }
            
            return meta.Proceed();
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