using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForTests.UseForVariableInCompileTimeExpresson
{
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