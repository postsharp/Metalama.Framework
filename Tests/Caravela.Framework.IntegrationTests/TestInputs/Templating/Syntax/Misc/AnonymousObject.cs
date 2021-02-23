using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.Misc.AnonymousObject
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private int Method(int a, int b)
        {
            return a + b;
        }
    }
}