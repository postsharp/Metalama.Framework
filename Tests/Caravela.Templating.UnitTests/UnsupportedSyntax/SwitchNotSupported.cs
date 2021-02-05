using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string SwitchNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        dynamic result;
        switch( target.Parameters.Count )
        {
            case 0:
                result = null;
                break;
            case 1:
                result = target.Parameters[0].Value;
                break;
            case 2:
                goto default;
            case 3:
                goto case 2;
            default:
                result = proceed();
                break;
        }
        return result;
    }
}
";

        private const string SwitchNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string SwitchNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task SwitchNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( SwitchNotSupported_Template, SwitchNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
