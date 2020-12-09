using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class CombinedTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public CombinedTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
