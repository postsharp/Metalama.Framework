using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private const string ReturnWithCast_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        object x = 1;
        return x;
    }
}
";

        private const string ReturnWithCast_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string ReturnWithCast_ExpectedOutput = @"";

        [Fact]
        public async Task ReturnWithCast()
        {
            var testResult = await this._testRunner.Run( new TestInput( ReturnWithCast_Template, ReturnWithCast_Target ) );
            testResult.AssertOutput( ReturnWithCast_ExpectedOutput );
        }
    }
}
