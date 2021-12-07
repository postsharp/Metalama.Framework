using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.OldSwitchMismatchScope
{
    enum SwitchEnum 
    {
        one = 1,
        two = 2,
    }

    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var i = meta.CompileTime(0);

            switch (i)
            {
                case (int)SwitchEnum.one:
                    Console.WriteLine("1");
                    break;
                case (int)SwitchEnum.two:
                    Console.WriteLine("2");
                    break;
                default:
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