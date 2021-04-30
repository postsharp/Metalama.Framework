using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.CompileTimeLinqSimpleLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var p = target.Parameters.Where(a => a.Name.Length > 8).Count();
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