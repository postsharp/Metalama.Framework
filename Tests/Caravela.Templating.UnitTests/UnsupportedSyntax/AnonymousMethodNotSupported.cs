using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string AnonymousMethodNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        Action<object> action =
        delegate (object p)
        {
            Console.WriteLine(p.ToString());
        };

        dynamic result = proceed();
        
        action(result);
        
        return result;
    }
}
";

        private const string AnonymousMethodNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string AnonymousMethodNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task AnonymousMethodNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( AnonymousMethodNotSupported_Template, AnonymousMethodNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
