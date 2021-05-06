using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.CompileTimeAnonymousObject
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var x = compileTime( new
            {
                Arg0 = target.Parameters[0].Name,
                Count = target.Parameters.Count
            } );


            Console.WriteLine(x.Arg0);
            
            dynamic result = proceed();
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