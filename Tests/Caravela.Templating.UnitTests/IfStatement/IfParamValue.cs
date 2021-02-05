
using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfParamValue_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        if (target.Parameters[0].Value == null)
        {
            throw new ArgumentNullException(target.Parameters[0].Name);
        }

        var p = target.Parameters[1];
        if (p.Value == null)
        {
            throw new ArgumentNullException(p.Name);
        }

        return proceed();
    }
}
";

        private const string IfParamValue_Target = @"
class TargetCode
{
    string Method(object a, object b)
    {
        return a.ToString() + b.ToString();
    }
}
";

        private const string IfParamValue_ExpectedOutput = @"{
    if (a == null)
    {
        throw new ArgumentNullException(""a"");
    }

    if (b == null)
    {
        throw new ArgumentNullException(""b"");
    }

    return a.ToString() + b.ToString();
}
";

        [Fact]
        public async Task IfParamValue()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfParamValue_Template, IfParamValue_Target ) );
            testResult.AssertOutput( IfParamValue_ExpectedOutput );
        }
    }
}
