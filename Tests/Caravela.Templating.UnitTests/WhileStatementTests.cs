using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class WhileStatementTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public WhileStatementTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
