// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            Assert.Equal( "\"Hel\\0lo\\\"\"", serializer.Serialize( "Hel\0lo\"" ).NormalizeWhitespace().ToString() );
            Assert.Equal( "\"Hello,\\n world!\"", serializer.Serialize( @"Hello,
 world!" ).NormalizeWhitespace().ToString().Replace( "\\r", "" ) );
            Assert.Equal( "\"Hello, world!\"", serializer.Serialize( $@"Hello, {"world"}!" ).NormalizeWhitespace().ToString() );
        }
    }
}