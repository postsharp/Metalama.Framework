using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.NameClashCompileTimeForEach
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            foreach (var p in target.Parameters)
            {
                string text = p.Name + " = " + p.Value;
                Console.WriteLine(text);
            }

            return proceed();
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