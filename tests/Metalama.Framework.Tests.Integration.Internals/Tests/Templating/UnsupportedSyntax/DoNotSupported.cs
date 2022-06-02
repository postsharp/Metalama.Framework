using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.UnsupportedSyntax.DoNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int i = 0;
            do
            {
                i++;
            } while (i < meta.Target.Parameters.Count);

            Console.WriteLine("Test result = " + i);

            dynamic? result = meta.Proceed();
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}