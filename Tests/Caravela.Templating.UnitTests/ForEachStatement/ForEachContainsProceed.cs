using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachContainsProceed_Template = @"
using System;
using System.Linq;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic  Template()
    {
        string[] parameterNames = new string[AdviceContext.Method.Parameters.Count];
        foreach (var p in parameterNames )
        {
           return AdviceContext.Proceed();
        }

         return null;
    }
}
";

        private const string ForEachContainsProceed_Target = @"using System;

class TargetCode
{
    void Method(int a, int bb)
    {
        Console.WriteLine( a + bb );
    }
}";

        private const string ForEachContainsProceed_ExpectedOutput = @"{
    string[] parameterNames = new string[2];
    foreach (var p in parameterNames)
    {
        Console.WriteLine(a + bb);
    }

    return null;
}";

        [Fact]
        public async Task ForEachContainsProceed()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachContainsProceed_Template, ForEachContainsProceed_Target ) );
            testResult.AssertOutput( ForEachContainsProceed_ExpectedOutput );
        }
    }
}
