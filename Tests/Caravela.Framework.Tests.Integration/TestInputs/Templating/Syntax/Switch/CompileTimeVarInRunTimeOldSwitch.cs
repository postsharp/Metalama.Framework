using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.OldSwitchChangeCompileTimeVarInRunTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var i = 0;
            var compileTimeVar = compileTime(1);

            switch (i)
            {
                case 0:
                    compileTimeVar += 1;
                    Console.WriteLine(compileTimeVar);
                    break;
                case 1:
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