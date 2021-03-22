using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.TestFramework.Templating;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.IntegrationTests.Highlighting
{
    public class HighlightingUnitTests : TemplateUnitTestBase
    {
        public HighlightingUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\Declarations" )]
        public Task Declarations( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\Identifiers" )]
        public Task Identifiers( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\MemberAccess" )]
        public Task MemberAccess( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\InvocationExpressions" )]
        public Task InvocationExpressions( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\IfStatements" )]
        public Task IfStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\ForEachStatements" )]
        public Task ForEachStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\ForStatements" )]
        public Task ForStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory( Skip = "Not supported yet." )]
        [FromDirectory( @"TestInputs\Highlighting\WhileStatements" )]
        public Task WhileStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );

        [Theory( Skip = "Not supported yet." )]
        [FromDirectory( @"TestInputs\Highlighting\DoWhileStatements" )]
        public Task DoWhileStatements( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );
    }
}
