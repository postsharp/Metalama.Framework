using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.AnonymousObject
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var x = new
            {
                A = meta.Target.Parameters[0].Value,
                B = meta.Target.Parameters[1].Value,
                Count = meta.Target.Parameters.Count
            };

            var y = new
            {
                Count = meta.Target.Parameters.Count
            };

            Console.WriteLine(x);
            Console.WriteLine(x.A);
            Console.WriteLine(x.Count);
            Console.WriteLine(y.Count);

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