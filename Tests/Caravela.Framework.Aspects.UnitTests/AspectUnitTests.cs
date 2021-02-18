using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Aspects.UnitTests
{

    public class AspectUnitTests : AspectUnitTestBase
    {
        public AspectUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( "Introductions\\Methods" )]
        public Task Introductions( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( "Overrides\\Methods" )]
        public Task Overrides( string testName ) => this.AssertTransformedSourceEqualAsync( testName );

        [Theory]
        [FromDirectory( "Samples" )]
        public Task Samples( string testName ) => this.AssertTransformedSourceEqualAsync( testName );
    }

}
