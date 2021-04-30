using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeParenthesizedLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Func<int, int> action = (int x) => x + 1;

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