using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string WhileNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        int i = 0;
        while ( i < target.Parameters.Count )
        {
            i++;
        }

        Console.WriteLine( ""Test result = "" + i );

        dynamic result = proceed();
        return result;
    }
}
";

        private const string WhileNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string WhileNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task WhileNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( WhileNotSupported_Template, WhileNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
