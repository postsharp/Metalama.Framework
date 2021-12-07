// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.Utilities;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class StringSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestString()
        {
            using var testContext = this.CreateSerializationTestContext( "" );

            Assert.Equal( "\"Hel\\0lo\\\"\"", testContext.Serialize( "Hel\0lo\"" ).NormalizeWhitespace().ToString() );

            Assert.Equal(
                "\"Hello,\\n world!\"",
                testContext.Serialize( "Hello,\n world!" )
                    .NormalizeWhitespace()
                    .ToString()
                    .ReplaceOrdinal( "\\r", "" ) );

            Assert.Equal( "\"Hello, world!\"", testContext.Serialize( $@"Hello, {"world"}!" ).NormalizeWhitespace().ToString() );
        }
    }
}