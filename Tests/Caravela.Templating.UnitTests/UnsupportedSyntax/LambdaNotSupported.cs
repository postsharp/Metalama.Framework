using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string LambdaNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [Template]
    dynamic OverrideMethod()
    {
        Action<object> action = (object p) =>
        {
            Console.WriteLine(p.ToString());
        };

        dynamic result = proceed();
        
        action(result);
        
        return result;
    }
}
";

        private const string LambdaNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string LambdaNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task LambdaNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( LambdaNotSupported_Template, LambdaNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
