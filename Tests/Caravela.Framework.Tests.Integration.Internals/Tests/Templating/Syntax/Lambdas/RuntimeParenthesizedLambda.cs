using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeParenthesizedLambda
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