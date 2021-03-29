using System;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.ForTests.UseForVariableInCompileTimeExpresson
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            for (int i = 0; i < target.Parameters.Count; i++)
            {
                Console.WriteLine(target.Parameters[i].Name);
            }

            dynamic result = proceed();
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