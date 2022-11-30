using System.Threading.Tasks;
using Metalama.Testing.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing
{
    public class _Runner : AspectTestSuite
    {
        public _Runner( ITestOutputHelper logger) : base( logger ) { }
        
        [Theory, CurrentDirectory]
        public Task Test( string f ) => this.RunTestAsync( f );
    }
}