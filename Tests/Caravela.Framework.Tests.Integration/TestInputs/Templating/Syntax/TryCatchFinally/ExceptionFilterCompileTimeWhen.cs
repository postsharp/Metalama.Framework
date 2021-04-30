using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.ExceptionFilterCompileTimeWhen
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var ctString = compileTime("DivideByZero");
            try
            {
                return 1;
            }
            catch (Exception e) when (e.GetType().Name.Contains(ctString))
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