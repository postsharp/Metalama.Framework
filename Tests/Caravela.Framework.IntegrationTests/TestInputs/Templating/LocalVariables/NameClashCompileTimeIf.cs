using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.NameClashCompileTimeIf
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}