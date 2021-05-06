using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.UnsuportedSyntax.StatementParenthesizedLambdaNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Action<object> action = (object p) =>
            {
                Console.WriteLine(p.ToString());
            };

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