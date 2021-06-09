using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Tests
{
    public class AllTests : CaravelaTestSuite
    {
        public AllTests( ITestOutputHelper logger) : base( logger ) { }
        
        [Theory, TestFiles("Aspects")]
        public void Aspects( string f ) => this.RunTest( f, "Aspects" );
        
        [Theory, TestFiles("Templating")]
        public void Templating( string f ) => this.RunTest( f, "Templating" );
        
        [Theory, TestFiles("Formatting")]
        public void Formatting( string f ) => this.RunTest( f, "Formatting" );
    }
}