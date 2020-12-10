using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedTests
    {
        private const string DoNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [Template]
    dynamic Template()
    {
        int i = 0;
        do
        {
            i++;
        } while ( i < AdviceContext.Method.Parameters.Count );

        Console.WriteLine( ""Test result = "" + i );

        dynamic result = AdviceContext.Proceed();
        return result;
    }
}
";

        private const string DoNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string DoNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task DoNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( DoNotSupported_Template, DoNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
