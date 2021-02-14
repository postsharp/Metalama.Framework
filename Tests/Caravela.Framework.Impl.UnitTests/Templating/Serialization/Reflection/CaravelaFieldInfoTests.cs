using System.Linq;
using System.Reflection;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Serialization;
using Caravela.Framework.Impl.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaFieldInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestField()
        {
            var code = "class Target { public int Field; }";
            var serialized = this.SerializeField( code );
            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

            TestExpression<FieldInfo>( code, CaravelaPropertyInfoTests.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Field", info.Name );
                Assert.Equal( typeof( int ), info.FieldType );
            } );
        }

        [Fact]
        public void TestFieldGeneric()
        {
            var code = "class Target<TKey> { public TKey[] Field; }";
            var serialized = this.SerializeField( code );
            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

            TestExpression<FieldInfo>( code, CaravelaPropertyInfoTests.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Field", info.Name );
                Assert.Equal( "TKey[]", info.FieldType.Name );
            } );
        }

        private string SerializeField( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Properties.Single( m => m.Name == "Field" );
            var actual = new CaravelaLocationInfoSerializer( new ObjectSerializers() ).Serialize( new CaravelaLocationInfo( (Field) single ) ).ToString();
            return actual;
        }

        public CaravelaFieldInfoTests( ITestOutputHelper helper ) : base( helper )
        {
        }
    }
}