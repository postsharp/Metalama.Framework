using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.DoNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = 0;
            do
            {
                i++;
            } while (i < target.Parameters.Count);

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