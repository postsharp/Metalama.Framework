// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Tests.UnitTests.Serialization.Assets;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class ArrayTests : SerializerTestsBase
    {
        [Fact]
        public void TestBasicArray()
        {
            this.AssertSerialization( "new global::System.Int32[]{0, 0, 0, 0}", new int[4] );
            this.AssertSerialization( "new global::System.Int32[]{10, 20, 30}", new[] { 10, 20, 30 } );

            this.AssertSerialization(
                "new global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.Mars.Moon[]{global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.Mars.Moon.Deimos}",
                new[] { Mars.Moon.Deimos } );
        }

        [Fact]
        public void TestArrayOfLists()
        {
            this.AssertSerialization(
                "new global::System.Collections.Generic.List<global::System.Int32>[]{new global::System.Collections.Generic.List<global::System.Int32>{2}}",
                new[] { new List<int> { 2 } } );
        }

        [Fact]
        public void TestArrayOfArrays()
        {
            this.AssertSerialization(
                "new global::System.Int32[][]{new global::System.Int32[]{1, 2}}",
                new[] { new[] { 1, 2 } } );
        }

        [Fact]
        public void TestMultiArray()
        {
            Assert.Throws<InvalidUserCodeException>( () => this.AssertSerialization( "new System.Int32[,]{{2}}", new[,] { { 2 } } ) );
        }

        private void AssertSerialization( string expected, object o )
        {
            var creationExpression = this.SerializationService.Serialize( o, this.SerializationContext ).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}