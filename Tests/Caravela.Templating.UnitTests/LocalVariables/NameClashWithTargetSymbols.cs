using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class LocalVariablesTests
    {
        private const string NameClashWithTargetSymbols_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var PI = 3.14d;
        Console.WriteLine(PI);
        var r = 42;
        Console.WriteLine(r);
        var area = r*r;
        Console.WriteLine(area);
        var StringBuilder = new object();        
        Console.WriteLine(StringBuilder.ToString());
        
        return proceed();
    }
}
";

        private const string NameClashWithTargetSymbols_Target = @"
using System.Text;
using static System.Math;

class TargetCode
{
    double Method(double r)
    {
        double area = PI*r*r;
        return area;
    }
}
";

        private const string NameClashWithTargetSymbols_ExpectedOutput = @"{
    var PI_1 = 3.14;
    Console.WriteLine(PI_1);
    var r_1 = 42;
    Console.WriteLine(r_1);
    var area_1 = r_1 * r_1;
    Console.WriteLine(area_1);
    var StringBuilder_1 = new object();
    Console.WriteLine(StringBuilder_1.ToString());
    {
        double area = PI * r * r;
        return area;
    }
}";

        [Fact]
        public async Task NameClashWithTargetSymbols()
        {
            var testResult = await this._testRunner.Run( new TestInput( NameClashWithTargetSymbols_Template, NameClashWithTargetSymbols_Target ) );
            testResult.AssertOutput( NameClashWithTargetSymbols_ExpectedOutput );
        }
    }
}
