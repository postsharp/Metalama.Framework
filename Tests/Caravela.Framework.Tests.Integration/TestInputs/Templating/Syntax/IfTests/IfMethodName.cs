using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfMethodName
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int b = meta.CompileTime(0);

            if (meta.Method.Name == "Method")
            {
                b = 1;
            }
            else
            {
                b = 2;
            }

            Console.WriteLine(b);

            return meta.Proceed();
        }
    }

    class TargetCode
    {
        void Method()
        {
        }
    }
}