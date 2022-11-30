using System.Threading.Tasks;
using Metalama.Testing.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics
{
    public class _Runner : AspectTestSuite
    {
        public _Runner( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [CurrentDirectory]
        public Task Tests( string f ) => RunTestAsync( f );
    }
}