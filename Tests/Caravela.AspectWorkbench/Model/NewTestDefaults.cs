namespace Caravela.AspectWorkbench.Model
{
    static class NewTestDefaults
    {
        public const string TemplateSource = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        return proceed();
    }
}
";

        public const string TargetSource = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        public const string EmptyUnitTest = @"using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{{
    public partial class {0}Tests
    {{
        private const string {1}_Template = @"""";

        private const string {1}_Target = @"""";

        private const string {1}_ExpectedOutput = @"""";

        [Fact]
        public async Task {1}()
        {{
            var testResult = await this._testRunner.Run( new TestInput( {1}_Template, {1}_Target ) );
            testResult.AssertOutput( {1}_ExpectedOutput );
        }}
    }}
}}
";

        public const string TestCategoryMainSource = @"using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{{
    public partial class {0}Tests
    {{
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public {0}Tests( ITestOutputHelper logger )
        {{
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }}
    }}
}}
";
    }
}
