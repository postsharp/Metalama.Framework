using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeParenthesizedLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Func<int, int> action = (int x) => x + 1;

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