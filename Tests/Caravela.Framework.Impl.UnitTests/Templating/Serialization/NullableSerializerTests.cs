using Caravela.Framework.Impl.Templating.Serialization;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class NullableSerializerTests
    {
        private readonly ObjectSerializers _serializers = new ObjectSerializers();

        [Fact]
        public void TestPrimitiveNullables()
        {
            this.AssertSerialization( "5F", (float?) 5F );
            this.AssertSerialization( "null", (float?) null );
        }
        
        [Fact]
        public void TestListOfNullables() 
        {
            List<float?> list = new List<float?>();
            list.Add( 5 );
            list.Add( null );
            this.AssertSerialization( "new System.Collections.Generic.List<System.Nullable<System.Single>>{5F, null}", list );
        }
        
        private void AssertSerialization( string expected, object o )
        {
            string creationExpression = this._serializers.SerializeToRoslynCreationExpression(o).NormalizeWhitespace().ToString();
            Assert.Equal( expected, creationExpression );
        }
    }
}