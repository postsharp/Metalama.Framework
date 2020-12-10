using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedTests
    {
        private const string ForNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [Template]
    dynamic Template()
    {
        for (int i = 0; i < AdviceContext.Method.Parameters.Count; i++)
        {
            Console.WriteLine( AdviceContext.Method.Parameters[i].Name );
        }

        dynamic result = AdviceContext.Proceed();
        return result;
    }
}
";

        private const string ForNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string ForNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task ForNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForNotSupported_Template, ForNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
