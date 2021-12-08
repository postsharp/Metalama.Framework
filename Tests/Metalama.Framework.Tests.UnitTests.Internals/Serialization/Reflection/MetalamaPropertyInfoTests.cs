// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Metalama.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class MetalamaPropertyInfoTests : ReflectionTestBase
    {
        public MetalamaPropertyInfoTests( ITestOutputHelper helper ) : base( helper ) { }

        [Fact]
        public void TestProperty()
        {
            var code = "class Target { public int Property {get;} }";
            var serialized = this.SerializeProperty( code );

            this.AssertEqual(
                @"new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Target).GetProperty(""Property"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
                {
                    Assert.Equal( "Property", info.Name );
                    Assert.Equal( typeof(int), info.PropertyType );
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
                @"new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Target<>).GetProperty(""Property"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
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
                @"new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Target).GetProperty(""Property"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
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
            var code = "class Target { public string this[int target] {get{return default;}} }";
            var serialized = this.SerializeIndexerWithTarget( code );

            this.AssertEqual(
                @"new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Target).GetProperty(""Item"", typeof(global::System.String), new global::System.Type[]{typeof(global::System.Int32)}))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
                {
                    Assert.Equal( "Item", info.Name );
                    Assert.Equal( typeof(string), info.PropertyType );
                    Assert.Null( info.SetMethod );
                    Assert.NotNull( info.GetMethod );
                    Assert.Single( info.GetIndexParameters() );
                } );
        }

        [Fact]
        public void TestIndexerOnString()
        {
            var code = "class Target { public string this[int target] {get{return default;}} }";

            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var stringType = (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(string) );
            var properties = stringType.Properties;
            var property = properties.Single( p => p.Name == "this[]" );
            var serialized = testContext.Serialize( CompileTimeFieldOrPropertyInfo.Create( (Property) property ) ).ToString();

            this.AssertEqual(
                @"new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(typeof(global::System.String).GetProperty(""Chars"", typeof(global::System.Char), new global::System.Type[]{typeof(global::System.Int32)}))",
                serialized );

            TestExpression<PropertyInfo>(
                code,
                StripLocationInfo( serialized ),
                info =>
                {
                    Assert.Equal( "Chars", info.Name );
                    Assert.Equal( typeof(char), info.PropertyType );
                    Assert.Null( info.SetMethod );
                    Assert.NotNull( info.GetMethod );
                    Assert.Single( info.GetIndexParameters() );
                } );
        }

        private string SerializeIndexerWithTarget( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var single = compilation.Types.Single( t => t.Name == "Target" ).Properties.Single( p => p.Parameters.Any( pp => pp.Name == "target" ) );
            var property = (Property) single;
            var actual = testContext.Serialize( CompileTimeFieldOrPropertyInfo.Create( property ) ).ToString();

            return actual;
        }

        public static string StripLocationInfo( string serialized )
        {
            const string prefix = "new global::Metalama.Framework.RunTime.FieldOrPropertyInfo(";

            return serialized.Substring( prefix.Length, serialized.Length - prefix.Length - 1 );
        }

        private string SerializeProperty( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var single = compilation.Types.Single( t => t.Name == "Target" ).Properties.Single( p => p.Name == "Property" );
            var property = (Property) single;
            var actual = testContext.Serialize( CompileTimeFieldOrPropertyInfo.Create( property ) ).ToString();

            return actual;
        }
    }
}