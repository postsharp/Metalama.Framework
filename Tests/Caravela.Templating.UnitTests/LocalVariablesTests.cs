using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class LocalVariablesTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public LocalVariablesTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
