using System;
using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.UnsupportedSyntax.LocalFuncNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            void LocalFunc(dynamic? p)
            {
                Console.WriteLine(p?.ToString());
            }

            dynamic? result = meta.Proceed();

            LocalFunc(result);

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