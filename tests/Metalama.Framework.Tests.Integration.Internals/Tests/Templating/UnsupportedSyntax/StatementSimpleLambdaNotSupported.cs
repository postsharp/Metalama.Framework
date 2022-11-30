using System;
using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.UnsupportedSyntax.StatementSimpleLambdaNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Action<object?> action = p =>
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