using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.OldSwitchRunTime
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