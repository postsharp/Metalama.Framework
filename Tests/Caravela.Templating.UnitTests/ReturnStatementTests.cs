using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class ReturnStatementTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public ReturnStatementTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
