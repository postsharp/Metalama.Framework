using Caravela.Framework.Impl;
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedTests
    {
        private const string AwaitNotSupported_Template = @"  
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class Aspect
{
    [Template]
    async Task<T> Template<T>()
    {
        await Task.Yield();

        dynamic result = AdviceContext.Proceed();
        return result;
    }
}
";

        private const string AwaitNotSupported_Target = @"
using System.Threading.Tasks;

class TargetCode
{
    async Task<int> Method( int a, int b )
    {
        return await Task.FromResult( a + b );
    }
}
";

        private const string AwaitNotSupported_ExpectedOutput = @"";

        [Fact]
        public async Task AwaitNotSupported()
        {
            var testResult = await this._testRunner.Run( new TestInput( AwaitNotSupported_Template, AwaitNotSupported_Target ) );
            testResult.AssertDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
