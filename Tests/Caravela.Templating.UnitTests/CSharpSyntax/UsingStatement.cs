using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string UsingStatement_Template = @"  
using System;
using System.Collections.Generic;
using System.IO;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        using(new MemoryStream())
        {
            dynamic result = proceed();
            return result;
        }
    }
}
";

        private const string UsingStatement_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string UsingStatement_ExpectedOutput = @"";

        [Fact]
        public async Task UsingStatement()
        {
            var testResult = await this._testRunner.Run( new TestInput( UsingStatement_Template, UsingStatement_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
