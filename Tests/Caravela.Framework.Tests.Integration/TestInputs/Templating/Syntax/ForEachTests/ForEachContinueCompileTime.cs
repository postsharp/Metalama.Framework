using System;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachContinueCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = compileTime(0);
            foreach (var p in target.Parameters)
            {
                if (p.Name.Length <= 1) continue;
                i++;
            }

            Console.WriteLine(i);

            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int bb)
        {
            return a + bb;
        }
    }
}