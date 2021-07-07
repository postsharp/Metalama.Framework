using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.RuntimeSimpleLambda2
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Action<object> action = a => Console.WriteLine(a.ToString());

            dynamic result = meta.Proceed();

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