// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaPropertyInfoTests : ReflectionTestBase
    {
        public CaravelaPropertyInfoTests( ITestOutputHelper helper ) : base( helper ) { }

        [Fact]
        public void TestProperty()
        {
            var code = "class Target { public int Property {get;} }";
            var serialized = this.SerializeProperty( code );

            this.AssertEqual(
                @"new global::Caravela.Framework.Code.FieldOrPropertyInfo(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Property"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
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

            this.AssertEqual(
                @"new global::Caravela.Framework.Code.FieldOrPropertyInfo(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).GetProperty(""Property"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
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

            this.AssertEqual(
                @"new global::Caravela.Framework.Code.FieldOrPropertyInfo(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Property"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
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

            this.AssertEqual(
                @"new global::Caravela.Framework.Code.FieldOrPropertyInfo(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target"")).GetProperty(""Item"", global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")), new global::System.Type[]{global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))}))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
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
            var stringType = (INamedType) compilation.Factory.GetTypeByReflectionType( typeof( string ) );
            var properties = stringType.Properties;
            var property = properties.Single( p => p.Name == "this[]" );
            var serialized = this.Serialize( CompileTimeFieldOrPropertyInfo.Create( (Property) property ) ).ToString();

            this.AssertEqual(
                @"new global::Caravela.Framework.Code.FieldOrPropertyInfo(global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")).GetProperty(""Chars"", global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Char"")), new global::System.Type[]{global::System.Type.GetTypeFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))}))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
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
            var actual = this.Serialize( CompileTimeFieldOrPropertyInfo.Create( property ) ).ToString();

            return actual;
        }

        public static string StripLocationInfo( string serialized )
        {
            const string prefix = "new global::Caravela.Framework.Code.FieldOrPropertyInfo(";

            return serialized.Substring( prefix.Length, serialized.Length - prefix.Length - 1 );
        }

        private string SerializeProperty( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Properties.Single( p => p.Name == "Property" );
            var property = (Property) single;
            var actual = this.Serialize( CompileTimeFieldOrPropertyInfo.Create( property ) ).ToString();

            return actual;
        }
    }
}