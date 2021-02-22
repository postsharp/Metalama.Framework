using Caravela.Framework.Impl.Serialization;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.UnitTests.Templating.Serialization
{
    public class StringSerializerTests
    {
        [Fact]
        public void TestString()
        {
            var serializer = new StringSerializer();
            Xunit.Assert.Equal( "\"Hel\\0lo\\\"\"", serializer.Serialize( "Hel\0lo\"" ).NormalizeWhitespace().ToString() );
            Xunit.Assert.Equal( "\"Hello,\\n world!\"", serializer.Serialize( @"Hello,
 world!" ).NormalizeWhitespace().ToString().Replace( "\\r", "" ) );
            Xunit.Assert.Equal( "\"Hello, world!\"", serializer.Serialize( $@"Hello, {"world"}!" ).NormalizeWhitespace().ToString() );
        }
    }
}