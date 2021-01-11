using Caravela.Framework.Impl.Templating.Serialization;
using System;
using System.Globalization;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class CultureInfoSerializerTests
    {
        [Fact]
        public void TestCzech()
        {
            var serializer = new CultureInfoSerializer();
            CultureInfo ci = new CultureInfo( "cs-CZ", true );
            Assert.Equal(@"new System.Globalization.CultureInfo(""cs-CZ"", true)", serializer.Serialize(ci).ToString());
        }
        [Fact]
        public void TestSlovakFalse()
        {
            var serializer = new CultureInfoSerializer();
            CultureInfo ci = new CultureInfo( "sk-SK", false );
            Assert.Equal(@"new System.Globalization.CultureInfo(""sk-SK"", false)", serializer.Serialize(ci).ToString());
        }
    }
}