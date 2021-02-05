using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string Yield_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    IEnumerable<int> Template()
    {
        yield return 1;
        
        if (target.Parameters.Count == 0)
        {
            yield break;
        }
        proceed();
    }
}
";

        private const string Yield_Target = @"
using System.Collections.Generic;

class TargetCode
{
    IEnumerable<int> Method(int a)
    {
        yield return a;
    }
}
";

        private const string Yield_ExpectedOutput = @"";

        [Fact]
        public async Task Yield()
        {
            var testResult = await this._testRunner.Run( new TestInput( Yield_Template, Yield_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
