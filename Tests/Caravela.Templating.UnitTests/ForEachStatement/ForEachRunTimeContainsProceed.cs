using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachRunTimeContainsProceed_Template = @"
using System;
using System.Linq;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        IEnumerable<int> array = Enumerable.Range(1, 2);
        foreach (var i in array)
        {
           return proceed();
        }

        return null;
    }
}
";

        private const string ForEachRunTimeContainsProceed_Target = @"using System;

class TargetCode
{
    void Method(int a, int bb)
    {
        Console.WriteLine( a + bb );
    }
}";

        private const string ForEachRunTimeContainsProceed_ExpectedOutput = @"{
    IEnumerable<int> array = Enumerable.Range(1, 2);
    foreach (var i in array)
    {
        Console.WriteLine(a + bb);
    }

    return;
}";

        [Fact]
        public async Task ForEachRunTimeContainsProceed()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachRunTimeContainsProceed_Template, ForEachRunTimeContainsProceed_Target ) );
            testResult.AssertOutput( ForEachRunTimeContainsProceed_ExpectedOutput );
        }
    }
}
