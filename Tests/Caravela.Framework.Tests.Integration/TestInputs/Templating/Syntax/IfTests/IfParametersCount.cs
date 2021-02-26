using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfParametersCount
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