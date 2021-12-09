using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.UnsupportedSyntax.UnsafeNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int i = meta.Target.Parameters.Count;
            unsafe
            {
                int* p = &i;

                *p = 42;
            }

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