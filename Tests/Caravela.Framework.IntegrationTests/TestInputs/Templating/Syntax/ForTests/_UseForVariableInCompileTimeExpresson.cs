using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForTests.UseForVariableInCompileTimeExpresson
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            for (int i = 0; i < target.Parameters.Count; i++)
            {
                Console.WriteLine(target.Parameters[i].Name);
            }

            dynamic result = proceed();
            return result;
        }
    }

    internal class TargetCode
    {
        private int Method(int a, int b)
        {
            return a + b;
        }
    }
}