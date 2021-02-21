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
        [FromDirectory( @"TestInputs\Templating" )]
        public Task All( string testName ) => this.AssertTransformedSourceEqualAsync( testName );
    }
}
