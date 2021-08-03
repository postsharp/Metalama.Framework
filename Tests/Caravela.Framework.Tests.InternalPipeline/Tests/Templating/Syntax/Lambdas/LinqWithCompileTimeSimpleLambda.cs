using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.LinqWithCompileTimeSimpleLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(5);

            var p = list.Where(a => a > meta.Target.Parameters.Count).Count();
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