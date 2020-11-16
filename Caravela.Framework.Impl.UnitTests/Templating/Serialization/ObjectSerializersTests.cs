using Caravela.Framework.Impl.Templating.Serialization;
using Microsoft.CodeAnalysis;
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
           this.AssertSerialization( "new List<System.Int32>{4, 6, 8}", new List<int>() { 4, 6, 8} );
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

        private void AssertSerialization( string expected, object o )
        {
            string creationExpression = this._serializers.SerializeToRoslynCreationExpression(o).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}