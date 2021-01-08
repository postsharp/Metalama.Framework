using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaFieldInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestField()
        {
            string code = "class Target { public int Field; }";
            string serialized = this.SerializeField( code );
            AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

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
            AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

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
            string actual = new CaravelaLocationInfoSerializer(new ObjectSerializers(), new CaravelaTypeSerializer()).Serialize( new CaravelaLocationInfo( single as Field ) ).ToString();
            return actual;
        }

        public CaravelaFieldInfoTests(ITestOutputHelper helper) : base(helper)
        {
        }
    }
}