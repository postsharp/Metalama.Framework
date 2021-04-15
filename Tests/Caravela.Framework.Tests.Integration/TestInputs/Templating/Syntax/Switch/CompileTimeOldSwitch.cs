using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.OldSwitchCompileTime
{
    [CompileTimeOnly]
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
            var i = compileTime(SwitchEnum.one);

            switch (i)
            {
                case SwitchEnum.one:
                    Console.WriteLine("1");
                    break;
                case SwitchEnum.two:
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