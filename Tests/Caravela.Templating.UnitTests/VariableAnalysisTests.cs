using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class VariableAnalysisTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public VariableAnalysisTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
