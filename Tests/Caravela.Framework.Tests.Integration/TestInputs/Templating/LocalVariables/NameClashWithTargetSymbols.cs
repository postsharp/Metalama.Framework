using System;
using System.Collections.Generic;

using System.Text;
using static System.Math;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.NameClashWithTargetSymbols
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var PI = 3.14d;
            Console.WriteLine(PI);
            var r = 42;
            Console.WriteLine(r);
            var area = r * r;
            Console.WriteLine(area);
            var StringBuilder = new object();
            Console.WriteLine(StringBuilder.ToString());

            return proceed();
        }
    }

    class TargetCode
    {
        double Method(double r)
        {
            double area = PI * r * r;
            return area;
        }
    }
}