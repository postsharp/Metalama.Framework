using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public IfStatementTests( ITestOutputHelper logger )
        {
            this._logger = logger;
            this._testRunner = new UnitTestRunner( this._logger );
        }
    }
}
