using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.TryCatchFinallyCompileTime
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
                Console.WriteLine(meta.Target.Parameters.Count);
                
            }
            finally
            {
                Console.WriteLine(meta.Target.Parameters.Count);
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