using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.TryCatchFinally.TryCatchFinallyRunTime
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}