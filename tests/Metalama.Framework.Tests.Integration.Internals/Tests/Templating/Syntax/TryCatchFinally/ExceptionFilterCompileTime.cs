using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.TryCatchFinally.ExceptionFilterCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int a = meta.Target.Parameters.Count;
            int b = 0;
            try
            {
                meta.InsertComment("comment");
                Console.WriteLine(a);
                
                var x = 100 / 1;
                var y = x / a;
                
            }
            catch(Exception e) when (e.GetType().Name.Contains("DivideByZero"))
            {
                meta.InsertComment("comment");
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