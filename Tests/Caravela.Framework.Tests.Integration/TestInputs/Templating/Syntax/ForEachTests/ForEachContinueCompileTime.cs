using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachContinueCompileTime
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = meta.CompileTime(0);
            foreach (var p in meta.Parameters)
            {
                if (p.Name.Length <= 1) continue;
                i++;
            }

            Console.WriteLine(i);

            dynamic result = meta.Proceed();
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