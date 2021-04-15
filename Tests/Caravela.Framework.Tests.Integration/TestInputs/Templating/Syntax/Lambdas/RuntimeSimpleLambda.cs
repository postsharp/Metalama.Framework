using System;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeSimpleLambda
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var a = 1;

            Action<int> action = a => a = a + 1;

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