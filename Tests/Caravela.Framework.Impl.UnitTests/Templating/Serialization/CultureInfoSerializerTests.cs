using System.Globalization;
using Caravela.Framework.Impl.Serialization;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class CultureInfoSerializerTests
    {
        [Fact]
        public void TestCzech()
        {
            var serializer = new CultureInfoSerializer();
            var ci = new CultureInfo( "cs-CZ", true );
            Assert.Equal( @"new System.Globalization.CultureInfo(""cs-CZ"", true)", serializer.Serialize( ci ).ToString() );
        }

        [Fact]
        public void TestSlovakFalse()
        {
            var serializer = new CultureInfoSerializer();
            var ci = new CultureInfo( "sk-SK", false );
            Assert.Equal( @"new System.Globalization.CultureInfo(""sk-SK"", false)", serializer.Serialize( ci ).ToString() );
        }
    }
}