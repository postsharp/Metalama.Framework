using Caravela.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects
{
    public class _Runner : TestSuite
    {
        public _Runner( ITestOutputHelper logger) : base( logger ) { }
        
        [Theory, CurrentDirectory("Introductions")]
        public void Introductions( string f ) => this.RunTest( f );
        
        [Theory, CurrentDirectory("Overrides")]
        public void Overrides( string f ) => this.RunTest( f );
        
        [Theory, CurrentDirectory]
        public void Other( string f ) => this.RunTest( f );
    }
}