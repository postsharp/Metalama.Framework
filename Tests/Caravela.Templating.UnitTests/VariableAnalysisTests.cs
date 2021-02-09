using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class VariableAnalysisTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public VariableAnalysisTests( ITestOutputHelper logger )
        {
            this._logger = logger;
            this._testRunner = new UnitTestRunner( this._logger );
        }
    }
}
