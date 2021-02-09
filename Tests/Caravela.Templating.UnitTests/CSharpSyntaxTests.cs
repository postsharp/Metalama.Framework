using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public CSharpSyntaxTests( ITestOutputHelper logger )
        {
            this._logger = logger;
            this._testRunner = new UnitTestRunner( this._logger );
        }
    }
}
