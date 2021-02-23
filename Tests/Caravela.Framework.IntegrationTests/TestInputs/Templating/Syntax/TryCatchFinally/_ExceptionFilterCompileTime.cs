using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.TryCatchFinally.ExceptionFilterCompileTime
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            int a = target.Parameters.Count;
            int b = 0;
            try
            {
                b = 100 / a;
            }
            catch (Exception e) when (e.GetType().Name.Contains("DivideByZero"))
            {
                b = 42;
            }

            Console.WriteLine(b);
            return proceed();
        }
    }

    internal class TargetCode
    {
        private int Method()
        {
            return 42;
        }
    }
}