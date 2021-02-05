using Caravela.Framework.Impl;
using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedSyntaxTests
    {
        private const string LockNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
    private static readonly object o = new object();
    
    [TestTemplate]
    dynamic Template()
    {
        dynamic result;
        lock (o)
        {
            result = proceed();
        }
        return result;
    }
}
";

        private const string LockNotSupported_Target = @"
class TargetCode
{
    int Method( int a, int b )
    {
        return a + b;
    }
}
";

        private const string LockNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task LockNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( LockNotSupported_Template, LockNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
