using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeParenthesizedLambda2
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Func<int, int, int> action = (int a, int b) => a + b;

            dynamic? result = meta.Proceed();

            result = action(result, 2);

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