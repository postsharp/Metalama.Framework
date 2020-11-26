using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization;
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
            string code = "class Target { public int Field; }";
            string serialized = this.SerializeField( code );
            Assert.Equal( @"new Caravela.Framework.LocationInfo(System.Reflection.FieldInfo.GetFieldFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeFieldHandle(""F:Target.Field""), Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")))", serialized );

            TestExpression<FieldInfo>( code, CaravelaPropertyInfoTests.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Field", info.Name );
                Assert.Equal( typeof(int), info.FieldType );
            } );
        }
        [Fact]
        public void TestFieldGeneric()
        {
            string code = "class Target<TKey> { public TKey[] Field; }";
            string serialized = this.SerializeField( code );
            Assert.Equal( @"new Caravela.Framework.LocationInfo(System.Reflection.FieldInfo.GetFieldFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeFieldHandle(""F:Target`1.Field""), Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")))", serialized );

            TestExpression<FieldInfo>( code, CaravelaPropertyInfoTests.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Field", info.Name );
                Assert.Equal( "TKey[]", info.FieldType.Name );
            } );
        }

        private string SerializeField( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IProperty single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Properties.GetValue().Single( m => m.Name == "Field" );
            string actual = new CaravelaLocationInfoSerializer(new ObjectSerializers()).Serialize( new CaravelaLocationInfo( single as Field ) ).ToString();
            return actual;
        }
    }
}