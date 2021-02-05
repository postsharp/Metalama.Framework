using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private const string ReturnObjectWithCast_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        object x = target.Parameters[0].Value;
        return x;
    }
}
";

        private const string ReturnObjectWithCast_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string ReturnObjectWithCast_ExpectedOutput = @"{
    object x = a;
    return (int)x;
}";

        [Fact]
        public async Task ReturnObjectWithCast()
        {
            var testResult = await this._testRunner.Run( new TestInput( ReturnObjectWithCast_Template, ReturnObjectWithCast_Target ) );
            testResult.AssertOutput( ReturnObjectWithCast_ExpectedOutput );
        }
    }
}
