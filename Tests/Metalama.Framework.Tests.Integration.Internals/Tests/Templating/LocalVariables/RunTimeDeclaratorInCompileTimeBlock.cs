using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.RunTimeDeclaratorInCompileTimeBlock
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            if (meta.Target.Parameters.Count > 0)
            {
                var x = 0;
                Console.WriteLine(x);
            }

            foreach (var p in meta.Target.Parameters)
            {
                var y = 0;
                Console.WriteLine(y);
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