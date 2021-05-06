using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.ExceptionFilterCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int a = meta.Parameters.Count;
            int b = 0;
            try
            {
                meta.Comment("comment");
                Console.WriteLine(a);
                
                var x = 100 / 1;
                var y = x / a;
                
            }
            catch(Exception e) when (e.GetType().Name.Contains("DivideByZero"))
            {
                meta.Comment("comment");
                b =  1;
            }

            Console.WriteLine(b);
            return meta.Proceed();
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