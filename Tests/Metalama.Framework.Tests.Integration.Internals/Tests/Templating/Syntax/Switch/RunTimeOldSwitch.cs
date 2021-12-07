using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.DefaultInOldSwitchRunTime
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
                default:
                    Console.WriteLine("Default");
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