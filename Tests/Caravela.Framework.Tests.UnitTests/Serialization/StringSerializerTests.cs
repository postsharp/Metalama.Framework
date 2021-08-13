// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class StringSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestString()
        {
            Assert.Equal( "\"Hel\\0lo\\\"\"", this.Serialize( "Hel\0lo\"" ).NormalizeWhitespace().ToString() );

            Assert.Equal(
                "\"Hello,\\n world!\"",
                this.Serialize( "Hello,\n world!" )
                    .NormalizeWhitespace()
                    .ToString()
                    .Replace( "\\r", "", StringComparison.Ordinal ) );

            Assert.Equal( "\"Hello, world!\"", this.Serialize( $@"Hello, {"world"}!" ).NormalizeWhitespace().ToString() );
        }
    }
}