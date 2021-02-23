#pragma warning disable CS8600, CS8603
using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.ReturnStatement.ReturnVoidProceedAndDefault
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            try
            {
                return proceed();
            }
            catch
            {
                return default;
            }
        }
    }

    internal class TargetCode
    {
        private void Method(int a, int b)
        {
            Console.WriteLine(a / b);
        }
    }
}