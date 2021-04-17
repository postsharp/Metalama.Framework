using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.ProceedInOldSwitchRunTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = 1;

            switch (i)
            {
                case 0:
                    Console.WriteLine("0");
                    break;
                case 1:
                    var x = proceed();
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