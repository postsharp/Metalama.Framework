using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.Misc.AnonymousObject
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var x = new
            {
                A = target.Parameters[0].Value,
                B = target.Parameters[1].Value,
                Count = target.Parameters.Count
            };

            var y = new
            {
                Count = target.Parameters.Count
            };

            Console.WriteLine(x);
            Console.WriteLine(x.A);
            Console.WriteLine(x.Count);
            Console.WriteLine(y.Count);

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