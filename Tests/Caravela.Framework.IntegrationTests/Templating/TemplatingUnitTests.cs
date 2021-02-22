using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Caravela.TestFramework.Templating;
using Caravela.UnitTestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.IntegrationTests.Templating
{
    public class TemplatingUnitTests : TemplateUnitTestBase
    {
        public TemplatingUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Syntax" )]
        public Task Syntax( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\LocalVariables" )]
        public Task LocalVariables( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Proceed" )]
        public Task Proceed( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Return" )]
        public Task Return( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\Samples" )]
        public Task Samples( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Templating\UnsupportedSyntax" )]
        public async Task UnsupportedSyntax( string testName )
        {
            var testResult = await this.RunTestAsync( testName );
            Assert.False( testResult.Success );
            testResult.AssertContainsDiagnosticId( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id );
        }
    }
}
