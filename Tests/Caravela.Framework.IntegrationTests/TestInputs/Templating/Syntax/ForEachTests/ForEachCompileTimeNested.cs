using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForEachTests.ForEachCompileTimeNested
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            IEnumerable<int> array = compileTime(Enumerable.Range(1, 2));

            foreach (int n in array)
            {
                foreach (var p in target.Parameters)
                {
                    if (p.Value <= n)
                    {
                        Console.WriteLine("Oops " + p.Name + " <= " + n);
                    }
                }
            }

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