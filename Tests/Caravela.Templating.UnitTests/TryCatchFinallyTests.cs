using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class TryCatchFinallyTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public TryCatchFinallyTests( ITestOutputHelper logger )
        {
            this._logger = logger;
            this._testRunner = new UnitTestRunner( this._logger );
        }
    }
}
