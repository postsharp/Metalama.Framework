using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.CompileTimeAnonymousObject
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = meta.CompileTime( new
            {
                Arg0 = meta.Target.Parameters[0].Name,
                Count = meta.Target.Parameters.Count
            } );


            Console.WriteLine(x.Arg0);
            
            dynamic? result = meta.Proceed();
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}