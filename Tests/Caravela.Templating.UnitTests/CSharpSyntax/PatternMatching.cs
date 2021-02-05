using Caravela.Framework.Impl;
using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string PatternMatching_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        dynamic result = proceed();
        
        switch(result)
        {
            case string s:
                Console.WriteLine(s);
                break;
            case int i when i < 0:
                throw new IndexOutOfRangeException();
            case var x:
                break;
        }
        
        return result;
    }
}
";

        private const string PatternMatching_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string PatternMatching_ExpectedOutput = @"";

        [Fact]
        public async Task PatternMatching()
        {
            var testResult = await this._testRunner.Run( new TestInput( PatternMatching_Template, PatternMatching_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
