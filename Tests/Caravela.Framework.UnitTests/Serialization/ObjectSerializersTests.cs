using System;
using System.Collections.Generic;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Serialization;
using EnumSpace;
using Microsoft.CodeAnalysis;
using Xunit;

#pragma warning disable SA1201 // Elements should appear in the correct order
#pragma warning disable SA1402
#pragma warning disable SA1403

namespace Caravela.Framework.UnitTests.Templating.Serialization
{
    public class ObjectSerializersTests
    {
        private readonly ObjectSerializers _serializers = new ObjectSerializers();

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
            this.AssertSerialization( "new System.Collections.Generic.List<System.Int32>{4, 6, 8}", new List<int>() { 4, 6, 8 } );
        }

        [Fact]
        public void TestString()
        {
            this.AssertSerialization( "\"hello\"", "hello" );
        }

        [Fact]
        public void TestInfiniteRecursion()
        {
            Assert.Throws<InvalidUserCodeException>( () =>
            {
                var o = new List<object>();
                o.Add( o );
                this._serializers.SerializeToRoslynCreationExpression( o );
            } );
        }

        [Fact]
        public void TestUnsupportedAnonymousType()
        {
            Assert.Throws<InvalidUserCodeException>( () => this._serializers.SerializeToRoslynCreationExpression( new { A = "F" } ) );
        }

        [Fact]
        public void TestNull()
        {
            this.AssertSerialization( "null", (object?) null );
        }

        private void AssertSerialization<T>( string expected, T? o )
        {
            var creationExpression = this._serializers.SerializeToRoslynCreationExpression( o ).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }

        [Fact]
        public void TestEnumsBasic()
        {
            this.AssertSerialization( "EnumSpace.World.Venus", World.Venus );
            this.AssertSerialization( "EnumSpace.Mars.Moon.Phobos", Mars.Moon.Phobos );
        }

        [Fact]
        public void TestNegativeEnum()
        {
            this.AssertSerialization( "(EnumSpace.LongEnum)(-1L)", (LongEnum) (-1) );
        }

        [Fact]
        public void TestEnumsFlags()
        {
            this.AssertSerialization( "(EnumSpace.WorldFeatures)(9UL)", WorldFeatures.Icy | WorldFeatures.Volcanic );
            this.AssertSerialization( "(EnumSpace.HumanFeatures)(9UL)", HumanFeatures.Tall | HumanFeatures.Wise );
            this.AssertSerialization( "EnumSpace.WorldFeatures.Icy", WorldFeatures.Icy );
        }

        [Fact]
        public void TestEnumsGenerics()
        {
            this.AssertSerialization( "(EnumSpace.Box<System.Int32>.Color)(12L)", Box<int>.Color.Blue | Box<int>.Color.Red );
            this.AssertSerialization( "EnumSpace.Box<System.Int32>.Color.Blue", Box<int>.Color.Blue );
            this.AssertSerialization( "EnumSpace.Box<System.Int32>.InnerBox.Shiny.Yes", Box<int>.InnerBox.Shiny.Yes );
        }

        [Fact]
        public void TestArray()
        {
            this.AssertSerialization( "new System.Int32[]{1, 2}", new[] { 1, 2 } );
        }

        [Fact]
        public void TestEnumsGenericsInGenericMethod()
        {
            this.GenericMethod<float>();
        }

        private void GenericMethod<TK>()
        {
            this.AssertSerialization( "EnumSpace.Box<System.Single>.Color.Blue", Box<TK>.Color.Blue );
        }
    }
}

namespace EnumSpace
{
    public enum LongEnum
    {
        First,
        Second
    }

    internal class Mars
    {
        public enum Moon
        {
            Phobos,
            Deimos
        }
    }

    internal class Box<T>
    {
        public T? Value { get; set; }

        public class InnerBox
        {
            public enum Shiny
            {
                Yes,
                No
            }
        }

        [Flags]
        public enum Color
        {
            Blue = 4,
            Red = 8
        }
    }

    [Flags]
    internal enum WorldFeatures : ulong
    {
        Icy = 1,
        Edenlike = 2,
        Poisonous = 4,
        Volcanic = 8
    }

    [Flags]
    internal enum HumanFeatures : byte
    {
        Tall = 1,
        Old = 2,
        Smart = 4,
        Wise = 8
    }

    internal enum World
    {
        Mercury,
        Venus
    }
}