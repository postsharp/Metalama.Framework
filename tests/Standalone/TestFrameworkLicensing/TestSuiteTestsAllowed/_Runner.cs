using System.Threading.Tasks;
using Metalama.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.TestSuiteTestsAllowed
{
    public class _Runner : TestSuite
    {
        public _Runner( ITestOutputHelper logger ) : base( logger ) { }
        
        [Theory, CurrentDirectory]
        public Task Other( string f ) => this.RunTestAsync( f );
    }
}