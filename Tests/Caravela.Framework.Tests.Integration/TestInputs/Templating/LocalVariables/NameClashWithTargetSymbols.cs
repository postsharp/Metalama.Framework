using System;
using static System.Math;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.Integration.Templating.LocalVariables.NameClashWithTargetSymbols
{
    [CompileTime]
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