using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.ExceptionFilterCompileTimeWhen
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var ctString = meta.CompileTime("DivideByZero");
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