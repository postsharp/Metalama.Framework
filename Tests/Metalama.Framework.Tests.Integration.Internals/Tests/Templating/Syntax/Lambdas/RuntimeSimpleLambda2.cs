using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.RuntimeSimpleLambda2
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Action<object?> action = a => Console.WriteLine(a?.ToString());

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