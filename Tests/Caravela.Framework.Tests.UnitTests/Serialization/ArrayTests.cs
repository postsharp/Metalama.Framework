// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Serialization;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class ArrayTests
    {
        private readonly ObjectSerializers _serializers = new();

        [Fact]
        public void TestBasicArray()
        {
            this.AssertSerialization( "new System.Int32[]{0, 0, 0, 0}", new int[4] );
            this.AssertSerialization( "new System.Int32[]{10, 20, 30}", new[] { 10, 20, 30 } );

            this.AssertSerialization(
                "new Caravela.Framework.Tests.UnitTests.Serialization.Mars.Moon[]{Caravela.Framework.Tests.UnitTests.Serialization.Mars.Moon.Deimos}",
                new[] { Mars.Moon.Deimos } );
        }

        [Fact]
        public void TestArrayOfLists()
        {
            this.AssertSerialization(
                "new System.Collections.Generic.List<System.Int32>[]{new System.Collections.Generic.List<System.Int32>{2}}",
                new[] { new List<int> { 2 } } );
        }

        [Fact]
        public void TestArrayOfArrays()
        {
            this.AssertSerialization(
                "new System.Int32[][]{new System.Int32[]{1, 2}}",
                new[] { new[] { 1, 2 } } );
        }

        [Fact]
        public void TestMultiArray()
        {
            Assert.Throws<InvalidUserCodeException>( () => this.AssertSerialization( "new System.Int32[,]{{2}}", new[,] { { 2 } } ) );
        }

        private void AssertSerialization( string expected, object o )
        {
            var creationExpression = this._serializers.SerializeToRoslynCreationExpression( o ).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}