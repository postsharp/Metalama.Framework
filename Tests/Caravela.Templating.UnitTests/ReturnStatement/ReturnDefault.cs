using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private const string ReturnDefault_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        try
        {
            dynamic result = proceed();
            return result;
        }
        catch
        {
            return default;
        }
    }
}
";

        private const string ReturnDefault_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return 42 / a;
    }
}
";

        private const string ReturnDefault_ExpectedOutput = @"{
    try
    {
        int result;
        result = 42 / a;
        return (int)result;
    }
    catch
    {
        return default;
    }
}
";

        [Fact]
        public async Task ReturnDefault()
        {
            var testResult = await this._testRunner.Run( new TestInput( ReturnDefault_Template, ReturnDefault_Target ) );
            testResult.AssertOutput( ReturnDefault_ExpectedOutput );
        }
    }
}
