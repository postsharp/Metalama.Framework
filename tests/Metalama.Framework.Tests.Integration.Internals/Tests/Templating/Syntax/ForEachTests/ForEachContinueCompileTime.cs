using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachContinueCompileTime
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
                if (p.Name.Length <= 1) continue;
                i++;
            }

            Console.WriteLine(i);

            dynamic? result = meta.Proceed();
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int bb)
        {
            return a + bb;
        }
    }
}