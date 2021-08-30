using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using System.Linq;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.CompileTimeLinqSimpleLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var p = meta.Target.Parameters.Where(a => a.Name.Length > 8).Count();
            Console.WriteLine(p);
            
            return meta.Proceed();
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