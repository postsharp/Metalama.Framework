using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.TestFramework.Templating;
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
        [FromDirectory( @"TestInputs\Templating\CSharpSyntax" )]
        public Task CSharpSyntax( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

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

        //[Theory]
        //[FromDirectory( @"TestInputs\Templating\UnsupportedSyntax" )]
        //public Task UnsupportedSyntax( string testName ) => this.AssertTransformedSourceEqualAsync( testName );
    }
}
