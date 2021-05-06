using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.TryCatchFinallyCompileTime
{
    [CompileTime]
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
                Console.WriteLine(target.Parameters.Count);
                
            }
            finally
            {
                Console.WriteLine(target.Parameters.Count);
            }

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