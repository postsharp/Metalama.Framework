using System.Threading.Tasks;
using Metalama.TestFramework;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.Integration.Tests.Templating.Syntax
{
    public class _Runner : TestSuite
    {
        public _Runner( ITestOutputHelper logger) : base( logger ) { }
        
        [Theory, CurrentDirectory]
        public Task Test( string f ) => this.RunTestAsync( f );
    }
}