// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.TestFramework;
using System;
using System.Linq;
using Xunit;

#pragma warning disable CA1018 // Mark attributes with AttributeUsageAttribute

namespace Caravela.Framework.Tests.UnitTests
{
    public class AttributeDeserializerTests : TestBase
    {
        public AttributeDeserializerTests()
        {
            // For the ease of testing, we need the custom attributes and helper classes nested here to be considered to 
            // belong to a system library so they can be shared between the compile-time code and the testing code.
            this.ServiceProvider.ReplaceServiceForTest<SystemTypeResolver>( new HackedSystemTypeResolver( this.ServiceProvider ) );
        }

        private object? GetDeserializedProperty( string property, string value, string? dependentCode = null )
        {
            var code = $@"[assembly: Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestAttribute( {property} = {value} )]";
            var compilation = CreateCompilationModel( code, dependentCode );

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, this.ServiceProvider );

            var attribute = compilation.Attributes.Single();
            DiagnosticList diagnosticList = new();

            if ( !loader.AttributeDeserializer.TryCreateAttribute( attribute, diagnosticList, out var deserializedAttribute ) )
            {
                throw new AssertionFailedException();
            }

            return deserializedAttribute.GetType().GetProperty( property ).AssertNotNull().GetValue( deserializedAttribute );
        }

        [Fact]
        public void TestPrimitiveTypes()
        {
            Assert.Equal( 5, this.GetDeserializedProperty( nameof(TestAttribute.Int32Property), "5" ) );
            Assert.Equal( "Zuzana", this.GetDeserializedProperty( nameof(TestAttribute.StringProperty), "\"Zuzana\"" ) );
            Assert.Equal( new[] { 5 }, this.GetDeserializedProperty( nameof(TestAttribute.Int32ArrayProperty), "new[]{5}" ) );
        }

        [Fact]
        public void TestBoxedTypes()
        {
            Assert.Equal( 5, this.GetDeserializedProperty( nameof(TestAttribute.ObjectProperty), "5" ) );
            Assert.Equal( "Zuzana", this.GetDeserializedProperty( nameof(TestAttribute.ObjectProperty), "\"Zuzana\"" ) );
            Assert.Equal( new[] { 5 }, this.GetDeserializedProperty( nameof(TestAttribute.ObjectProperty), "new[]{5}" ) );
        }

        [Fact]
        public void TestEnums()
        {
            Assert.Equal(
                TestEnum.A,
                this.GetDeserializedProperty(
                    nameof(TestAttribute.EnumProperty),
                    "Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum.A" ) );

            Assert.Equal(
                TestEnum.A,
                this.GetDeserializedProperty(
                    nameof(TestAttribute.ObjectProperty),
                    "Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum.A" ) );

            Assert.Equal(
                new[] { TestEnum.A },
                this.GetDeserializedProperty(
                    nameof(TestAttribute.EnumArrayProperty),
                    "new[]{Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum.A}" ) );
        }

        [Fact]
        public void TestTypes()
        {
            Assert.Equal( typeof(int), this.GetDeserializedProperty( nameof(TestAttribute.TypeProperty), "typeof(int)" ) );

            Assert.Equal(
                typeof(TestEnum),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum)" ) );

            Assert.Equal(
                new[] { typeof(TestEnum), typeof(int) },
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeArrayProperty),
                    "new[]{typeof(Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum),typeof(int)}" ) );

            Assert.Equal(
                new[] { typeof(TestEnum), typeof(int) },
                this.GetDeserializedProperty(
                    nameof(TestAttribute.ObjectArrayProperty),
                    "new[]{typeof(Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum),typeof(int)}" ) );
        }

        [Fact]
        public void TestNonRunTimeType()
        {
            var dependentCode = "public class MyExternClass {} public enum MyExternEnum { A, B }";
            var typeValue = this.GetDeserializedProperty( nameof(TestAttribute.TypeProperty), "typeof(MyExternClass)", dependentCode );
            Assert.Equal( "MyExternClass", Assert.IsType<CompileTimeType>( typeValue ).FullName );

            var objectValue = this.GetDeserializedProperty( nameof(TestAttribute.ObjectProperty), "MyExternEnum.B", dependentCode );
            Assert.Equal( 1, objectValue );
        }

        [Fact]
        public void TestParams()
        {
            object Deserialize( string args )
            {
                var code = $@"[assembly: Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestParamsAttribute( {args} )]";
                var compilation = CreateCompilationModel( code );

                using UnloadableCompileTimeDomain domain = new();
                var loader = CompileTimeProjectLoader.Create( domain, this.ServiceProvider );

                var attribute = compilation.Attributes.Single();

                if ( !loader.AttributeDeserializer.TryCreateAttribute( attribute, new DiagnosticList(), out var deserializedAttribute ) )
                {
                    throw new AssertionFailedException();
                }

                return ((TestParamsAttribute) deserializedAttribute).Value;
            }

            Assert.Equal( new[] { "a", "b" }, Deserialize( "\"a\", \"b\"" ) );
            Assert.Equal( new[] { "a" }, Deserialize( "\"a\"" ) );
            Assert.Equal( new[] { 1, 2 }, Deserialize( "1, 2" ) );
            Assert.Equal( new[] { 1 }, Deserialize( "1" ) );
            Assert.Equal( new[] { typeof(int), typeof(string) }, Deserialize( "typeof(int), typeof(string)" ) );
            Assert.Equal( new[] { typeof(int) }, Deserialize( "typeof(int)" ) );
        }

        public enum TestEnum
        {
            A,
            B
        }

        public class TestAttribute : Attribute
        {
            public int Int32Property { get; set; }

            public string? StringProperty { get; set; }

            public object? ObjectProperty { get; set; }

            public TestEnum EnumProperty { get; set; }

            public Type? TypeProperty { get; set; }

            public int[]? Int32ArrayProperty { get; set; }

            public object[]? ObjectArrayProperty { get; set; }

            public Type[]? TypeArrayProperty { get; set; }

            public TestEnum[]? EnumArrayProperty { get; set; }
        }

        public class TestParamsAttribute : Attribute
        {
            public object Value { get; private set; }

            public TestParamsAttribute( params string[] p ) { this.Value = p; }

            public TestParamsAttribute( params int[] p ) { this.Value = p; }

            public TestParamsAttribute( params Type[] p ) { this.Value = p; }

            public TestParamsAttribute( params object[] p ) { this.Value = p; }
        }

        private class HackedSystemTypeResolver : SystemTypeResolver
        {
            public HackedSystemTypeResolver( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

            protected override bool IsStandardAssemblyName( string assemblyName )
                => base.IsStandardAssemblyName( assemblyName ) || assemblyName == this.GetType().Assembly.GetName().Name;
        }
    }
}