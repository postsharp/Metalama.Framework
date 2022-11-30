using System.Threading.Tasks;
using Metalama.Testing.Framework;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.TestSuiteTestsAllowed
{
    public class _Runner : AspectTestSuite
    {
        public _Runner( ITestOutputHelper logger ) : base( logger ) { }
        
        [Theory, CurrentDirectory]
        public Task Other( string f ) => this.RunTestAsync( f );
    }
}