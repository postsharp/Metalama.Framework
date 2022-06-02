using System.Threading.Tasks;
using Metalama.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects
{
    public class _Runner : TestSuite
    {
        public _Runner( ITestOutputHelper logger) : base( logger ) { }
        
        [Theory, CurrentDirectory("Introductions")]
        public Task Introductions( string f ) => this.RunTestAsync( f );
        
        [Theory, CurrentDirectory("Overrides")]
        public Task Overrides( string f ) => this.RunTestAsync( f );
        
        [Theory, CurrentDirectory]
        public Task Other( string f ) => this.RunTestAsync( f );
    }
}