// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization
{
    public class ListSerializerTests : SerializerTestsBase
    {
        [Fact]
        public void TestEmptyList()
        {
            this.AssertSerialization( "new global::System.Collections.Generic.List<global::System.Single>{}", new List<float>() );
        }

        [Fact]
        public void TestBasicList()
        {
            this.AssertSerialization( "new global::System.Collections.Generic.List<global::System.Single>{1F, 2F, 3F}", new List<float> { 1, 2, 3 } );
        }

        [Fact]
        public void TestListInList()
        {
            this.AssertSerialization(
                "new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Int32>>{new global::System.Collections.Generic.List<global::System.Int32>{1}}",
                new List<List<int>> { new() { 1 } } );
        }

        [Fact]
        public void TestInfiniteRecursion()
        {
            var l = new List<IList>();
            l.Add( l );

            using var testContext = this.CreateSerializationTestContext( "" );

            Assert.Throws<DiagnosticException>( () => testContext.Serialize( l ) );
        }

        private void AssertSerialization<T>( string expected, List<T> o )
        {
            using var testContext = this.CreateSerializationTestContext( "" );
            var creationExpression = testContext.Serialize( o ).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}