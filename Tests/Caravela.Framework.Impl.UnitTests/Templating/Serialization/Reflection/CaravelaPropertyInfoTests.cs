using System.Linq;
using System.Reflection;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaPropertyInfoTests : ReflectionTestBase
    {
        private readonly CaravelaLocationInfoSerializer _caravelaLocationInfoSerializer;

        public CaravelaPropertyInfoTests( ITestOutputHelper helper ) : base( helper )
        {
            this._caravelaLocationInfoSerializer = new CaravelaLocationInfoSerializer( new ObjectSerializers() );
        }

        [Fact]
        public void TestProperty()
        {
            var code = "class Target { public int Property {get;} }";
            var serialized = this.SerializeProperty( code );
            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Property"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

            TestExpression<PropertyInfo>( code, StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Property", info.Name );
                Assert.Equal( typeof( int ), info.PropertyType );
                Assert.Null( info.SetMethod );
                Assert.NotNull( info.GetMethod );
            } );
        }

        [Fact]
        public void TestGenericProperty()
        {
            var code = "class Target<T> { public T Property {get;} }";
            var serialized = this.SerializeProperty( code );
            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).GetProperty(""Property"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

            TestExpression<PropertyInfo>( code, StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Property", info.Name );
                Assert.Equal( "T", info.PropertyType.Name );
                Assert.Null( info.SetMethod );
                Assert.NotNull( info.GetMethod );
            } );
        }

        [Fact]
        public void TestNonAutomaticProperty()
        {
            var code = "class Target { public string Property {get{return default;}set{}} }";
            var serialized = this.SerializeProperty( code );
            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Property"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

            TestExpression<PropertyInfo>( code, StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Property", info.Name );
                Assert.Equal( typeof( string ), info.PropertyType );
                Assert.NotNull( info.SetMethod );
                Assert.NotNull( info.GetMethod );
            } );
        }

        [Fact]
        public void TestIndexer()
        {
            var code = "class Target { public string this[int target] {get{return default;}} }";
            var serialized = this.SerializeIndexerWithTarget( code );

            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Item"", System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")), new System.Type[]{System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))}))", serialized );

            TestExpression<PropertyInfo>( code, StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Item", info.Name );
                Assert.Equal( typeof( string ), info.PropertyType );
                Assert.Null( info.SetMethod );
                Assert.NotNull( info.GetMethod );
                Assert.Single( info.GetIndexParameters() );
            } );
        }

        [Fact]
        public void TestIndexerOnString()
        {
            var code = "class Target { public string this[int target] {get{return default;}} }";
            var compilation = CreateCompilation( code );
            var referencedTypes = compilation.DeclaredAndReferencedTypes;
            var stringType = referencedTypes.Single( t => t.Name == "String" );
            var properties = stringType.Properties;
            var property = properties.Single( p => p.Name == "this[]" );
            var serialized = this._caravelaLocationInfoSerializer.Serialize( new CaravelaLocationInfo( (Property) property ) ).ToString();
            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")).GetProperty(""Chars"", System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Char"")), new System.Type[]{System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))}))", serialized );

            TestExpression<PropertyInfo>( code, StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Chars", info.Name );
                Assert.Equal( typeof( char ), info.PropertyType );
                Assert.Null( info.SetMethod );
                Assert.NotNull( info.GetMethod );
                Assert.Single( info.GetIndexParameters() );
            } );
        }

        private string SerializeIndexerWithTarget( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Properties.Single( p => p.Parameters.Any( pp => pp.Name == "target" ) );
            var property = (Property) single;
            var actual = this._caravelaLocationInfoSerializer.Serialize( new CaravelaLocationInfo( property ) ).ToString();
            return actual;
        }

        public static string StripLocationInfo( string serialized )
        {
            return serialized.Substring( "new Caravela.Framework.LocationInfo(".Length, serialized.Length - "new Caravela.Framework.LocationInfo(".Length - 1 );
        }

        private string SerializeProperty( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Properties.Single( p => p.Name == "Property" );
            var property = (Property) single;
            var actual = this._caravelaLocationInfoSerializer.Serialize( new CaravelaLocationInfo( property ) ).ToString();
            return actual;
        }
    }
}