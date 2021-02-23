#pragma warning disable CS8600, CS8603
using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.ForEachTests.ForEachRunTimeContainsProceed
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            IEnumerable<int> array = Enumerable.Range(1, 2);
            foreach (var i in array)
            {
                return proceed();
            }

            return null;
        }
    }

    internal class TargetCode
    {
        private void Method(int a, int bb)
        {
            Console.WriteLine(a + bb);
        }
    }
}