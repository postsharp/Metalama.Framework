using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string LocalFuncNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        void LocalFunc(dynamic p)
        {
            Console.WriteLine(p.ToString());
        }

        dynamic result = proceed();
        
        LocalFunc(result);
        
        return result;
    }
}
";

        private const string LocalFuncNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string LocalFuncNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task LocalFuncNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( LocalFuncNotSupported_Template, LocalFuncNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
