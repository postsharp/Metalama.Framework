using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.RunTimeDeclaratorInCompileTimeBlock
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}