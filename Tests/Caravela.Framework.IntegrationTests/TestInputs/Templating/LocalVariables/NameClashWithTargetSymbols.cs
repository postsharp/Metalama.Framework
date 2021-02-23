using System;
using static System.Math;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.NameClashWithTargetSymbols
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private double Method(double r)
        {
            double area = PI * r * r;
            return area;
        }
    }
}