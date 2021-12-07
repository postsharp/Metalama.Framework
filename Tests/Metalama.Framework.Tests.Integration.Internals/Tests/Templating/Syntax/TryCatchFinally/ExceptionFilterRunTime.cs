using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.ExceptionFilterRunTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            try
            {
                Console.WriteLine(meta.Target.Parameters.Count);
                return meta.Proceed();
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