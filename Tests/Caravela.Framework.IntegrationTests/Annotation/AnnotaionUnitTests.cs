using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.TestFramework.Templating;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.IntegrationTests.Highlighting
{
    public class AnnotaionUnitTests : TemplateUnitTestBase
    {
        public AnnotaionUnitTests( ITestOutputHelper logger ) : base( logger )
        {
        }

        [Theory]
        [FromDirectory( @"TestInputs\Highlighting\ClassDeclaration" )]
        public Task ClassDeclaration( string testName ) => this.AssertTriviasPreservedByAnnotator( testName );
    }
}
