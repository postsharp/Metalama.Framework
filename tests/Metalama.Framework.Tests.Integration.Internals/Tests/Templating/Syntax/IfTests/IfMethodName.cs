using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.IfTests.IfMethodName
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int b = meta.CompileTime(0);

            if (meta.Target.Method.Name == "Method")
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