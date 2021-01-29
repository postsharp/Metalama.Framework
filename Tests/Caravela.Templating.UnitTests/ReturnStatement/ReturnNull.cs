
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private const string ReturnNull_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic OverrideMethod()
    {
        var a = target.Parameters[0];
        var b = target.Parameters[1];
        if (a.Value == null || b.Value == null)
        {
            return null;
        }
        dynamic result = proceed();
        return result;
    }
}
";

        private const string ReturnNull_Target = @"
class TargetCode
{
    string Method(string a, string b)
    {
        return a + b;
    }
}
";

        private const string ReturnNull_ExpectedOutput = @"{
    if (a == null || b == null)
    {
        return null;
    }

    string result;
    result = a + b;
    return (string)result;
}
";

        [Fact]
        public async Task ReturnNull()
        {
            var testResult = await this._testRunner.Run( new TestInput( ReturnNull_Template, ReturnNull_Target ) );
            testResult.AssertOutput( ReturnNull_ExpectedOutput );
        }
    }
}
