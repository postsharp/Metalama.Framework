using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.CompileSimpleLambda2
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            Action<object?> action = a => Console.WriteLine(a?.ToString());

            var result = meta.CompileTime(1);

            action(result);

            return meta.Proceed();
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