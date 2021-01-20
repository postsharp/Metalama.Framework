using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string SimpleLambdaNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [Template]
    dynamic Template()
    {
        Action<object> action =
        p =>
        {
            Console.WriteLine(p.ToString());
        };

        dynamic result = proceed();
        
        action(result);
        
        return result;
    }
}
";

        private const string SimpleLambdaNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string SimpleLambdaNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task SimpleLambdaNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( SimpleLambdaNotSupported_Template, SimpleLambdaNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
