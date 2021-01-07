using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class ProceedTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public ProceedTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
