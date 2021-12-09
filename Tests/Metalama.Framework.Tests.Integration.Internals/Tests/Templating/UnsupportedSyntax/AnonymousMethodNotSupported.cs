using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.UnsupportedSyntax.AnonymousMethodNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Action<object?> action =
            delegate (object? p)
            {
                Console.WriteLine(p?.ToString());
            };

            dynamic? result = meta.Proceed();

            action(result);

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