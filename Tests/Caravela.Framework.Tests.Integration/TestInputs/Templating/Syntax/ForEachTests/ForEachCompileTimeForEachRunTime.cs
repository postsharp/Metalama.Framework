using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForEachTests.ForEachCompileTimeForEachRunTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            IEnumerable<int> array = Enumerable.Range(1, 2);

            foreach (var p in target.Parameters)
            {
                foreach (int n in array)
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