using System.Threading.Tasks;
using Metalama.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers
{
    public class _Runner : TestSuite
    {
        public _Runner( ITestOutputHelper logger ) : base( logger ) { }

        [Theory]
        [CurrentDirectory]
        public Task All( string f ) => RunTestAsync( f );
    }
}