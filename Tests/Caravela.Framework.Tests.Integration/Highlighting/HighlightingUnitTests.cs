using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Highlighting
{
    public class HighlightingUnitTests : HighlightingUnitTestsBase
    {
        public HighlightingUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting" )]
        public Task All( string testName ) => this.AssertHighlightedSourceEqualAsync( testName );
        
    }
}
