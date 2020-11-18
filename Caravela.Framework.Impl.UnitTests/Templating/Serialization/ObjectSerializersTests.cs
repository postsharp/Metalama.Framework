using Caravela.Framework.Impl.Templating.Serialization;
using EnumSpace;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class ObjectSerializersTests
    {
        readonly ObjectSerializers _serializers = new ObjectSerializers();

        [Fact]
        public void TestInt()
        {
            this.AssertSerialization( "42", 42);
        }

        [Fact]
        public void TestListInt()
        {
           this.AssertSerialization( "new System.Collections.Generic.List<System.Int32>{4, 6, 8}", new List<int>() { 4, 6, 8} );
        }
        [Fact]
        public void TestString()
        {
            this.AssertSerialization( "\"hello\"", "hello" );
        }
        
        [Fact]
        public void TestInfiniteRecursion()
        {
            Assert.Throws<CaravelaException>( () =>
            {
                List<object> o = new List<object>();
                o.Add( o );
                this._serializers.SerializeToRoslynCreationExpression( o );
            } );
        }
        
        [Fact]
        public void TestUnsupportedAnonymousType()
        {
            Assert.Throws<CaravelaException>( () =>
            {
                this._serializers.SerializeToRoslynCreationExpression( new {A = "F"} );
            } );
        }

        [Fact]
        public void TestNull()
        {
            this.AssertSerialization( "null", null );
        }
       
        private void AssertSerialization( string expected, object o )
        {
            string creationExpression = this._serializers.SerializeToRoslynCreationExpression(o).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
        
        [Fact]
        public void TestEnumsBasic()
        {
            this.AssertSerialization( "EnumSpace.World.Venus", World.Venus );
            this.AssertSerialization( "EnumSpace.Mars.Moon.Phobos", Mars.Moon.Phobos );
        }
        
        [Fact]
        public void TestEnumsFlags()
        {
            this.AssertSerialization( "(EnumSpace.WorldFeatures)9UL", WorldFeatures.Icy | WorldFeatures.Volcanic );
            this.AssertSerialization( "(EnumSpace.HumanFeatures)9UL", HumanFeatures.Tall | HumanFeatures.Wise );
            this.AssertSerialization( "EnumSpace.WorldFeatures.Icy", WorldFeatures.Icy );
        }
        
        [Fact]
        public void TestEnumsGenerics()
        {
            this.AssertSerialization( "(EnumSpace.Box<System.Int32>.Color)12UL", Box<int>.Color.Blue | Box<int>.Color.Red );
            this.AssertSerialization( "EnumSpace.Box<System.Int32>.Color.Blue", Box<int>.Color.Blue );
        }  
        
        [Fact]
        public void TestEnumsGenericsInGenericMethod()
        {
            this.GenericMethod<float>();
        }

        private void GenericMethod<TK>()
        {
            this.AssertSerialization( "EnumSpace.Box<TK>.Color.Blue", Box<TK>.Color.Blue );
        }
    }
}

namespace EnumSpace
{
    class Mars
    {
        public enum Moon
        {
            Phobos,
            Deimos
        }
    }

    class Box<T>
    {
        public enum Color
        {
            Blue = 4,
            Red = 8
        }
    }

    [Flags]
    enum WorldFeatures : ulong
    {
        Icy = 1,
        Edenlike = 2,
        Poisonous = 4,
        Volcanic = 8
    } 
    [Flags]
    enum HumanFeatures : byte
    {
        Tall = 1,
        Old = 2,
        Smart = 4,
        Wise = 8
    }
    enum World
    {
        Mercury,
        Venus
    }
}