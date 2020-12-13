using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class TryCatchFinallyTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public TryCatchFinallyTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
