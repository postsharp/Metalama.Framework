using System.Threading.Tasks;
using Metalama.Testing.AspectTesting;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.AspectTests.Tests.LiveTemplates
{
    public class _Runner : AspectTestClass
    {
        public _Runner( ITestOutputHelper logger) : base( logger ) { }
        
        [Theory, CurrentDirectory]
        public Task Test( string f ) => this.RunTestAsync( f );
    }
}