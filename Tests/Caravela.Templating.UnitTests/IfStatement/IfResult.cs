
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfResult_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [Template]
  dynamic Template()
  {
        dynamic result = AdviceContext.Proceed();

        if (result == null)
        {
            return """";
        }
        
        return result;
  }
}
";

        private const string IfResult_Target = @"
class TargetCode
{
    string Method(object a)
    {
        return a?.ToString();
    }
}
";

        private const string IfResult_ExpectedOutput = @"
{
    string result;
    result = a?.ToString();
    if (result == null)
    {
        return """";
    }

    return result;
}
";

        [Fact]
        public async Task IfResult()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfResult_Template, IfResult_Target ) );
            testResult.AssertOutput( IfResult_ExpectedOutput );
        }
    }
}
