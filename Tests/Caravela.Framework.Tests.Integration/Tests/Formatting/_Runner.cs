using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Tests.Formatting
{
    public class _Runner : TestSuite
    {
        public _Runner( ITestOutputHelper logger) : base( logger ) { }
        
        [Theory, CurrentDirectory]
        public void Test( string f ) => this.RunTest( f );
    }
}