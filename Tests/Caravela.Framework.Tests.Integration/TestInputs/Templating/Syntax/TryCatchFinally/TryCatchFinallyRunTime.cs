using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.TryCatchFinallyRunTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            try
            {
                Console.WriteLine("try");
                dynamic result = proceed();
                Console.WriteLine("success");
                return result;
            }
            catch
            {
                Console.WriteLine("exception");
                throw;
            }
            finally
            {
                Console.WriteLine("finally");
            }
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