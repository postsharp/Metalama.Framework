using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.UnsupportedSyntax.UnsafeNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = target.Parameters.Count;
            unsafe
            {
                int* p = &i;

                *p = 42;
            }

            Console.WriteLine("Test result = " + i);

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