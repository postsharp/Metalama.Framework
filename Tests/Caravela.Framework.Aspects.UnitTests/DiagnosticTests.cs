using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Aspects.UnitTests
{
    public class DiagnosticTests : AspectUnitTestBase
    {
        
        public DiagnosticTests(ITestOutputHelper logger) : base(logger)
        {
        }

        [Fact]
        public async Task ReportFromInitialize()
        {
            var testResult = await this.RunPipelineAsync( "Diagnostics\\ReportFromInitialize.cs" );
            Assert.False( testResult.Success );
            Assert.Contains( testResult.Diagnostics.Where( d => d.Severity != DiagnosticSeverity.Hidden ), d => d.Id == "MY001" );
        }
        
        [Fact]
        public async Task ReportFromTemplate()
        {
            var testResult = await this.RunPipelineAsync( "Diagnostics\\ReportFromTemplate.cs" );
            Assert.False( testResult.Success );
            Assert.Contains( testResult.Diagnostics.Where( d => d.Severity != DiagnosticSeverity.Hidden ), d => d.Id == "MY001" );
        }

    }
}