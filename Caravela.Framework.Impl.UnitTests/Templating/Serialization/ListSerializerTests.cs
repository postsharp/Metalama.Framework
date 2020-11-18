using Caravela.Framework.Impl.Templating.Serialization;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class ListSerializerTests
    {
        private ListSerializer _serializer;

        public ListSerializerTests()
        {
            ObjectSerializers os = new ObjectSerializers();
            this._serializer = new ListSerializer( os );
        }

        [Fact]
        public void TestEmptyList()
        {
            this.AssertSerialization( "new System.Collections.Generic.List<System.Single>{}", new List<float>());
        }
        [Fact]
        public void TestBasicList()
        {
            this.AssertSerialization( "new System.Collections.Generic.List<System.Single>{1F, 2F, 3F}", new List<float>(){1,2,3});
        }
        [Fact]
        public void TestListInList()
        {
            this.AssertSerialization( "new System.Collections.Generic.List<System.Collections.Generic.List<System.Int32>>{new System.Collections.Generic.List<System.Int32>{1}}", new List<List<int>>(){new List<int>{1}});
        }

        [Fact]
        public void TestInfiniteRecursion()
        {
            var l = new List<IList>();
            l.Add( l );

            Assert.Throws<CaravelaException>( () =>
            {
                this._serializer.Serialize( l );
            } );
        }

        private void AssertSerialization<T>( string expected, List<T> o )
        {
            string creationExpression = this._serializer.Serialize(o).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}