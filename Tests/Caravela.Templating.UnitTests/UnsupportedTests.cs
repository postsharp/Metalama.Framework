using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class UnsupportedTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public UnsupportedTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
