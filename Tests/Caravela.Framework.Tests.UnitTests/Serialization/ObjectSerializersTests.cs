// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Tests.UnitTests.Serialization.Assets;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

// ReSharper disable IdentifierTypo

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class ObjectSerializersTests : SerializerTestsBase
    {
        [Fact]
        public void TestInt()
        {
            this.AssertSerialization( "42", 42 );
        }

        [Fact]
        public void TestNullable()
        {
            int? i = 42;
            this.AssertSerialization( "42", i );
        }

        [Fact]
        public void TestListInt()
        {
            this.AssertSerialization( "new global::System.Collections.Generic.List<global::System.Int32>{4, 6, 8}", new List<int> { 4, 6, 8 } );
        }

        [Fact]
        public void TestString()
        {
            this.AssertSerialization( "\"hello\"", "hello" );
        }

        [Fact]
        public void TestInfiniteRecursion()
        {
            Assert.Throws<InvalidUserCodeException>(
                () =>
                {
                    var o = new List<object>();
                    o.Add( o );
                    this.Serialize( o );
                } );
        }

        [Fact]
        public void TestUnsupportedAnonymousType()
        {
            Assert.Throws<InvalidUserCodeException>( () => this.Serialize( new { A = "F" } ) );
        }

        [Fact]
        public void TestNull()
        {
            this.AssertSerialization( "null", (object?) null );
        }

        private void AssertSerialization<T>( string expected, T? o )
        {
            var creationExpression = this.Serialize( o ).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }

        [Fact]
        public void TestEnumsBasic()
        {
            this.AssertSerialization( "global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.World.Venus", World.Venus );
            this.AssertSerialization( "global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.Mars.Moon.Phobos", Mars.Moon.Phobos );
        }

        [Fact]
        public void TestNegativeEnum()
        {
            this.AssertSerialization( "(global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.LongEnum)(-1L)", (LongEnum) (-1) );
        }

        [Fact]
        public void TestEnumsFlags()
        {
            this.AssertSerialization(
                "(global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.WorldFeatures)(9UL)",
                WorldFeatures.Icy | WorldFeatures.Volcanic );

            this.AssertSerialization(
                "(global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.HumanFeatures)(9UL)",
                HumanFeatures.Tall | HumanFeatures.Wise );

            this.AssertSerialization( "global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.WorldFeatures.Icy", WorldFeatures.Icy );
        }

        [Fact]
        public void TestEnumsNestedInGenerics()
        {
            this.AssertSerialization(
                "(global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.Box<global::System.Int32>.Color)(12L)",
                Box<int>.Color.Blue | Box<int>.Color.Red );

            this.AssertSerialization(
                "global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.Box<global::System.Int32>.Color.Blue",
                Box<int>.Color.Blue );
        }

        [Fact]
        public void TestEnumsTwiceNestedInGenerics()
        {
            this.AssertSerialization(
                "global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.Box<global::System.Int32>.InnerBox.Shiny.Yes",
                Box<int>.InnerBox.Shiny.Yes );
        }

        [Fact]
        public void TestArray()
        {
            this.AssertSerialization( "new global::System.Int32[]{1, 2}", new[] { 1, 2 } );
        }

        [Fact]
        public void TestEnumsGenericsInGenericMethod()
        {
            this.GenericMethod<float>();
        }

        private void GenericMethod<TK>()
        {
            this.AssertSerialization(
                "global::Caravela.Framework.Tests.UnitTests.Serialization.Assets.Box<global::System.Single>.Color.Blue",
                Box<TK>.Color.Blue );
        }
    }
}