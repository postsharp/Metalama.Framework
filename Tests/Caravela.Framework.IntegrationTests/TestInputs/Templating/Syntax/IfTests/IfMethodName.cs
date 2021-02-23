using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.IfTests.IfMethodName
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            int b = compileTime(0);

            if (target.Method.Name == "Method")
            {
                b = 1;
            }
            else
            {
                b = 2;
            }

            Console.WriteLine(b);

            return proceed();
        }
    }

    internal class TargetCode
    {
        private void Method()
        {
        }
    }
}