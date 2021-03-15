using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.RunTimeDeclaratorInCompileTimeBlock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            if (target.Parameters.Count > 0)
            {
                var x = 0;
                Console.WriteLine(x);
            }

            foreach (var p in target.Parameters)
            {
                var y = 0;
                Console.WriteLine(y);
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