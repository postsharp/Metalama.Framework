using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class DynamicMetaMemberTests
    {
        private const string Parameter_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        var parameter0 = TemplateContext.target.Parameters[0];
        var parameter1 = TemplateContext.target.Parameters[1];
        var tmp = parameter0.Value;
        parameter0.Value = parameter1.Value;
        parameter1.Value = tmp;
        return default;
    }
}
";

        private const string Parameter_Target = @"
class TargetCode
{
    void Method( ref int i, ref int j )
    {
    }
}
";

        private const string Parameter_ExpectedOutput = @"{
    var tmp = i;
    i = j;
    j = tmp;
    return;
}";

        [Fact]
        public async Task Parameter()
        {
            var testResult = await this._testRunner.Run( new TestInput( Parameter_Template, Parameter_Target ) );
            testResult.AssertOutput( Parameter_ExpectedOutput );
        }
    }
}
