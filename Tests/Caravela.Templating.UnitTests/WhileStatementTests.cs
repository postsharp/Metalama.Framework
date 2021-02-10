using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class WhileStatementTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public WhileStatementTests( ITestOutputHelper logger )
        {
            this._logger = logger;
            this._testRunner = new UnitTestRunner( this._logger );
        }
    }
}
