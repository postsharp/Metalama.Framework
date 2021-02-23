using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.TryCatchFinally.ExceptionFilterRunTime
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
            catch (Exception e) when (e.GetType().Name.Contains("DivideByZero"))
            {
                return -1;
            }
        }
    }

    internal class TargetCode
    {
        private int Method(int a)
        {
            return 42 / a;
        }
    }
}