using System;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.TryCatchFinallyRunTime
{
    [CompileTime]
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