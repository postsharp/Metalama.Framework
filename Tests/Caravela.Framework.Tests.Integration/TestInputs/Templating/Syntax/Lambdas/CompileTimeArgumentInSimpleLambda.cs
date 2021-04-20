using System;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.CompileSimpleLambda2
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Action<object> action = a => Console.WriteLine(a.ToString());

            var result = compileTime(1);

            action(result);

            return proceed();
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