using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.NameClashCompileTimeIf
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var n = target.Parameters.Count; // build-time
            object y = target.Parameters[0].Value; // run-time

            if (n == 1)
            {
                var x = 0;
                Console.WriteLine(x);
            }

            if (y == null)
            {
                var x = 1;
                Console.WriteLine(x);
            }

            if (n == 1)
            {
                var x = 2;
                Console.WriteLine(x);
            }

            if (y == null)
            {
                var x = 3;
                Console.WriteLine(x);
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