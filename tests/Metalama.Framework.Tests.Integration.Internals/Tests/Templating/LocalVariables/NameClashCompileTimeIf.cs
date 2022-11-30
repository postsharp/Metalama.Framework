using System;
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.NameClashCompileTimeIf
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var n = meta.Target.Parameters.Count; // build-time
            object? y = meta.Target.Parameters[0].Value; // run-time

            if (n == 1)
            {
                var x = 0;
                Console.WriteLine(x);
            }

            if (y == null)
            {
                var x = 1;
                Console.WriteLine(x);
            }

            if (n == 1)
            {
                var x = 2;
                Console.WriteLine(x);
            }

            if (y == null)
            {
                var x = 3;
                Console.WriteLine(x);
            }

            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}