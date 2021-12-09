using System;
using System.Text;
using static System.Math;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.NameClashWithTargetSymbols
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var PI = 3.14d;
            Console.WriteLine(PI);
            var r = 42;
            Console.WriteLine(r);
            var area = r * r;
            Console.WriteLine(area);
            var StringBuilder = new object();
            Console.WriteLine(StringBuilder.ToString());

            return meta.Proceed();
        }
    }

    class TargetCode
    {
        double Method(double r)
        {
            StringBuilder stringBuilder = new();
            double area = PI * r * r;
            return area;
        }
    }
}