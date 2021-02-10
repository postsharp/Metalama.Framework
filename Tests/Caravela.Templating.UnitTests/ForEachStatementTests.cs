using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public ForEachStatementTests( ITestOutputHelper logger )
        {
            this._logger = logger;
            this._testRunner = new UnitTestRunner( this._logger );
        }
    }
}
