using System;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.StatementSimpleLambdaNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Action<object> action = p =>
            {
                Console.WriteLine(p.ToString());
            };

            dynamic result = proceed();

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