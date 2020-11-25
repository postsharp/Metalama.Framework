using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaPropertyInfoTests : TestBase
    {
        [Fact]
        public void TestProperty()
        {
            string code = "class Target { public int Property {get;} }";
            string serialized = this.SerializeProperty( code );
            Assert.Equal( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Property""))", serialized );

            TestExpression<PropertyInfo>( code, this.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Property", info.Name );
                Assert.Equal( typeof(int), info.PropertyType );
                Assert.Null( info.SetMethod );
                Assert.NotNull( info.GetMethod );
            } );
        }
        
        [Fact]
        public void TestNonAutomaticProperty()
        {
            string code = "class Target { public string Property {get{return default;}set{}} }";
            string serialized = this.SerializeProperty( code );
            Assert.Equal( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Property""))", serialized );

            TestExpression<PropertyInfo>( code, this.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Property", info.Name );
                Assert.Equal( typeof(string), info.PropertyType );
                Assert.NotNull( info.SetMethod );
                Assert.NotNull( info.GetMethod );
            } );
        }
        
        [Fact]
        public void TestIndexer()
        {
            string code = "class Target { public string this[int target] {get{return default;}} }";
            string serialized = this.SerializeIndexerWithTarget( code );
            Assert.Equal( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Item"", System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")), new System.Type[]{System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))}))", serialized );

            TestExpression<PropertyInfo>( code, this.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Item", info.Name );
                Assert.Equal( typeof(string), info.PropertyType );
                Assert.Null( info.SetMethod );
                Assert.NotNull( info.GetMethod );
                Assert.Equal( 1, info.GetIndexParameters().Length );
            } );
        }

        [Fact]
        public void TestRealIndexer()
        {
            Type t = typeof(FakeTarget);
            PropertyInfo p = t.GetProperty( "this[]" );
            PropertyInfo p2 = t.GetProperty( "this[]", typeof(string), new[] {typeof(int)} );
        }

        class FakeTarget
        {
            public string this[int target] { get { return default; } }
        }

        private string SerializeIndexerWithTarget( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IProperty single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Properties.GetValue().Single( p => p.Parameters.Any(pp => pp.Name == "target") );
            Property p = (single as Property)!;
            string actual = new CaravelaLocationInfoSerializer(new ObjectSerializers()).Serialize( new CaravelaLocationInfo( p ) ).ToString();
            return actual;
        }

        private string StripLocationInfo( string serialized )
        {
            return serialized.Substring( "new Caravela.Framework.LocationInfo(".Length, serialized.Length - "new Caravela.Framework.LocationInfo(".Length - 1 );
        }

        private string SerializeProperty( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IProperty single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Properties.GetValue().Single( p => p.Name == "Property" );
            Property p = (single as Property)!;
            string actual = new CaravelaLocationInfoSerializer(new ObjectSerializers()).Serialize( new CaravelaLocationInfo( p ) ).ToString();
            return actual;
        }
    }
}