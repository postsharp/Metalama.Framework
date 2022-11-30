using System;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachParameter
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            int i = meta.CompileTime(0);
            foreach (var p in meta.Target.Parameters)
            {
                i++;
            }

            Console.WriteLine(i);

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