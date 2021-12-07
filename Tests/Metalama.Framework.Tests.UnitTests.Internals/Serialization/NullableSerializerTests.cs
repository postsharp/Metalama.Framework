// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class NullableSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestPrimitiveNullables()
        {
            this.AssertSerialization( "5F", (float?) 5F );
            this.AssertSerialization( "null", (float?) null );
        }

        [Fact]
        public void TestListOfNullables()
        {
            var list = new List<float?> { 5, null };
            this.AssertSerialization( "new global::System.Collections.Generic.List<global::System.Single?>{5F, null}", list );
        }

        private void AssertSerialization<T>( string expected, T? o )
        {
            using var testContext = this.CreateSerializationTestContext( "" );
            var creationExpression = testContext.Serialize( o ).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}