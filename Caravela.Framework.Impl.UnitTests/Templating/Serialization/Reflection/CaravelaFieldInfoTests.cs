using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaFieldInfoTests : TestBase
    {
        [Fact]
        public void TestField()
        {
            string code = "class Target { public int Target; }";
            string serialized = this.SerializeField( code );
            Assert.Equal( @"xxxx", serialized );

            TestExpression<LocationInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.FieldInfo.Name );
                Assert.Equal( typeof(int), info.FieldInfo.FieldType );
            } );
        }
        [Fact]
        public void TestFieldGeneric()
        {
            string code = "class Target<TKey> { public TKey[] Target; }";
            string serialized = this.SerializeField( code );
            Assert.Equal( @"xxxx", serialized );

            TestExpression<LocationInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.FieldInfo.Name );
                Assert.Equal( "TKey[]", info.FieldInfo.FieldType.Name );
            } );
        }

        private string SerializeField( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IProperty single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Properties.GetValue().Single( m => m.Name == "Target" );
            string actual = new LocationInfoSerializer().Serialize( new LocationInfo( (FieldInfo)null ) ).ToString();
            return actual;
        }
    }
}