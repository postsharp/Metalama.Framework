// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Serialization;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class ListSerializerTests
    {
        private readonly ListSerializer _serializer;

        public ListSerializerTests()
        {
            var os = new SyntaxSerializationService();
            this._serializer = new ListSerializer( os );
        }

        [Fact]
        public void TestEmptyList()
        {
            this.AssertSerialization( "new System.Collections.Generic.List<System.Single>{}", new List<float>() );
        }

        [Fact]
        public void TestBasicList()
        {
            this.AssertSerialization( "new System.Collections.Generic.List<System.Single>{1F, 2F, 3F}", new List<float> { 1, 2, 3 } );
        }

        [Fact]
        public void TestListInList()
        {
            this.AssertSerialization(
                "new System.Collections.Generic.List<System.Collections.Generic.List<System.Int32>>{new System.Collections.Generic.List<System.Int32>{1}}",
                new List<List<int>> { new() { 1 } } );
        }

        [Fact]
        public void TestInfiniteRecursion()
        {
            var l = new List<IList>();
            l.Add( l );

            Assert.Throws<InvalidUserCodeException>( () => this._serializer.SerializeObject( l ) );
        }

        private void AssertSerialization<T>( string expected, List<T> o )
        {
            var creationExpression = this._serializer.SerializeObject( o ).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}