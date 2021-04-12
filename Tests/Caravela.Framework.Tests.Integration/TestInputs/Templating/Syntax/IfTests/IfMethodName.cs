using System;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfMethodName
{
    [CompileTime]
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