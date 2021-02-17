using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.IfStatement.IfMethodName
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        void Method()
        {
        }
    }
}