using System;
using Metalama.Framework.Aspects;
using System.Linq;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.CompileTimeLinqSimpleLambda
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