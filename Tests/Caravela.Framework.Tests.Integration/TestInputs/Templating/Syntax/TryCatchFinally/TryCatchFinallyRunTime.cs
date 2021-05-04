#pragma warning disable CS0162

using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.TryCatchFinallyRunTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
        var x = compileTime(0);
            try
            {
                Console.WriteLine("try");
                dynamic result = proceed();
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