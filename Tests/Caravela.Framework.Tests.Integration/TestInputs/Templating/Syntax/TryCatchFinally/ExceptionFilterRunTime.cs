using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.TryCatchFinally.ExceptionFilterRunTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        int Method(int a)
        {
            return 42 / a;
        }
    }
}