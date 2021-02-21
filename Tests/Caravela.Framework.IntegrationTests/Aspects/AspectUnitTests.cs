using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.TestFramework.Aspects;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.IntegrationTests.Aspects
{
    public class AspectUnitTests : AspectUnitTestBase
    {
        public AspectUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Aspects\Samples" )]
        public Task Samples( string testName ) => this.AssertTransformedSourceEqualAsync( testName );
    }
}
