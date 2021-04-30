using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.LinqWithCompileTimeSimpleLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(5);

            var p = list.Where(a => a > target.Parameters.Count).Count();
            Console.WriteLine(p);
            
            return proceed();
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