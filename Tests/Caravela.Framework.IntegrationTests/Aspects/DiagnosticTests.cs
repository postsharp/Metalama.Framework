using System.Linq;
using System.Threading.Tasks;
using Caravela.TestFramework.Aspects;
using Microsoft.CodeAnalysis;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.IntegrationTests.Aspects
{
    public class DiagnosticTests : AspectUnitTestBase
    {

        public DiagnosticTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Fact]
        public async Task ReportFromInitialize()
        {
            var testResult = await this.RunPipelineAsync( @"TestInputs\Aspects\Diagnostics\ReportFromInitialize.cs" );
            Assert.False( testResult.Success );
            Assert.Contains( testResult.Diagnostics.Where( d => d.Severity != DiagnosticSeverity.Hidden ), d => d.Id == "MY001" );
        }

        [Fact]
        public async Task SkipWithoutError()
        {
            var testResult = await this.RunPipelineAsync( @"TestInputs\Aspects\Diagnostics\SkipWithoutError.cs" );
            Assert.True( testResult.Success );
            Assert.DoesNotContain( "This code should not be emitted.", testResult.TransformedTargetSource?.ToString() );
        }

        [Fact]
        public async Task ReportFromTemplate()
        {
            var testResult = await this.RunPipelineAsync( @"TestInputs\Aspects\Diagnostics\ReportFromTemplate.cs" );
            Assert.False( testResult.Success );
            Assert.Contains( testResult.Diagnostics.Where( d => d.Severity != DiagnosticSeverity.Hidden ), d => d.Id == "MY001" );
        }
    }
}