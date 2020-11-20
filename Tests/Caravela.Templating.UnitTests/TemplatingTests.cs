using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Templating.UnitTests
{
    public partial class MiscTests
    {
        private readonly ITestOutputHelper _logger;
        private readonly UnitTestRunner _testRunner;

        public MiscTests( ITestOutputHelper logger )
        {
            _logger = logger;
            _testRunner = new UnitTestRunner( _logger );
        }
    }
}
