using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeParenthesizedLambda2
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Func<int, int, int> action = (int a, int b) => a + b;

            dynamic result = proceed();

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