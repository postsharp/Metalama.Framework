#pragma warning disable CS0162

using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.TryCatchFinallyRunTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime(0);
            try
            {
                Console.WriteLine("try");
                dynamic? result = meta.Proceed();
                Console.WriteLine("success");
                return result;
            }
            catch
            {
                Console.WriteLine("exception " + x);
                throw;
            }
            finally
            {
                Console.WriteLine("finally");
            }
            
            Console.WriteLine(x);
            
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