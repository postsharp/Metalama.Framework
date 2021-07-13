using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.ForEachTests.ForEachCompileTimeNested
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            IEnumerable<int> array = meta.CompileTime(Enumerable.Range(1, 2));

            foreach (int n in array)
            {
                foreach (var p in meta.Parameters)
                {
                    if (p.Value <= n)
                    {
                        Console.WriteLine("Oops " + p.Name + " <= " + n);
                    }
                }
            }

            dynamic result = meta.Proceed();
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