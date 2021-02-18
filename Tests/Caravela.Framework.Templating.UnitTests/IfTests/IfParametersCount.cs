using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.IfTests.IfParametersCount
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            bool b = compileTime(false);

            if (target.Parameters.Count > 0)
            {
                b = true;
            }
            else
            {
                b = false;
            }

            Console.WriteLine(b);

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