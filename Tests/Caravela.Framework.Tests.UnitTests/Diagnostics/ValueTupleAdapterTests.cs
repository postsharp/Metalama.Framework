using Caravela.Framework.Impl.Diagnostics;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Diagnostics
{
    public class ValueTupleAdapterTests
    {
        [Fact]
        public void TestValueTupleAdapter()
        {
            Assert.Equal( new object[] { 1, "2" }, ValueTupleAdapter.ToArray( (1, "2") ) );
        }
    }
}