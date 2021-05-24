using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.LocalVariables.CompileTimeDeclaratorInCompileTimeBlock
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            if (meta.Parameters.Count > 0)
            {
                var x = meta.CompileTime(0);
                Console.WriteLine(x);
            }

            if (meta.Parameters.Count > 1)
            {
                var x = meta.CompileTime(1);
                Console.WriteLine(x);
            }

            foreach (var p in meta.Parameters)
            {
                var y = meta.CompileTime(0);
                Console.WriteLine(y);
            }

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