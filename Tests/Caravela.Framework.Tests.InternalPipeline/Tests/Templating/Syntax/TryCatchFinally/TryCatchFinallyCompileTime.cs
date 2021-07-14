using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.TryCatchFinallyCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int n = meta.CompileTime(1);
            try
            {
                n = 2;
            }
            catch
            {
                Console.WriteLine(meta.Parameters.Count);
                
            }
            finally
            {
                Console.WriteLine(meta.Parameters.Count);
            }

            return meta.Proceed();
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