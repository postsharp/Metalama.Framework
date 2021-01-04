using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string GotoNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [Template]
    dynamic Template()
    {
        dynamic result = AdviceContext.Proceed();
        
        if (result != null) goto end;
        
        return default;
        
end:        
        return result;
    }
}
";

        private const string GotoNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string GotoNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task GotoNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( GotoNotSupported_Template, GotoNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
