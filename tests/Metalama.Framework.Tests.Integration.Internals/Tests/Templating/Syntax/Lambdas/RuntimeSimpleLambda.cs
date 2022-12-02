using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeSimpleLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Func<int, int> action = x => x + 1;

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