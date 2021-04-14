using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.OldSwitchMismatchScope
{
    enum SwitchEnum 
    {
        one = 1,
        two = 2,
    }

    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var i = compileTime(0);

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
            
            return proceed();
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