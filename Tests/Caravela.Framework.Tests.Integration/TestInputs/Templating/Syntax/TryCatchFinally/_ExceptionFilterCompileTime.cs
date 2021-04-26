using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.ExceptionFilterCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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

    class TargetCode
    {
        int Method()
        {
            return 42;
        }
    }
}