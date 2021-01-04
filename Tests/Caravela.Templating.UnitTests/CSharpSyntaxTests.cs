using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public CSharpSyntaxTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
