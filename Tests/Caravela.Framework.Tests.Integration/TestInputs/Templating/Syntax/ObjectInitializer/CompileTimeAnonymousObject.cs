using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.CompileTimeAnonymousObject
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var x = meta.CompileTime( new
            {
                Arg0 = meta.Parameters[0].Name,
                Count = meta.Parameters.Count
            } );


            Console.WriteLine(x.Arg0);
            
            dynamic result = meta.Proceed();
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