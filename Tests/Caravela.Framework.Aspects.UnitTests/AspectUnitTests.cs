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
        [FromDirectory( "Samples" )]
        public Task Samples( string testName ) => this.RunTestAsync( testName );
    }


}
