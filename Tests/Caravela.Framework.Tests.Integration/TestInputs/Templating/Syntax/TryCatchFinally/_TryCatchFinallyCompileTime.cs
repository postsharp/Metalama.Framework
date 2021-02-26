using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.TryCatchFinally.TryCatchFinallyCompileTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int n = compileTime(1);
            try
            {
                n = 2;
            }
            catch
            {
                n = 3;
            }
            finally
            {
                n = 4;
            }

            target.Parameters[0].Value = n;
            return proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}