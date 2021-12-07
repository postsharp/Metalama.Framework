// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.ReflectionMocks;
using Metalama.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

#pragma warning disable CA1018 // Mark attributes with AttributeUsageAttribute

namespace Metalama.Framework.Tests.UnitTests.CompileTime
{
    public class AttributeDeserializerTests : TestBase
    {
        public AttributeDeserializerTests() : base( p => p.WithService( new HackedSystemTypeResolver( p ) ) )
        {
            // For the ease of testing, we need the custom attributes and helper classes nested here to be considered to 
            // belong to a system library so they can be shared between the compile-time code and the testing code.
        }

        private object? GetDeserializedProperty( string property, string value, string? dependentCode = null, string? additionalCode = "" )
        {
            using var testContext = this.CreateTestContext();

            var code = $@"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestAttribute( {property} = {value} )]"
                       + " enum RunTimeEnum { Value = 1}"
                       + " class GenericRunTimeType<T> {}"
                       + " struct GenericStruct {} "
                       + additionalCode;

            var compilation = testContext.CreateCompilationModel( code, dependentCode );

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

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
        public void TestField()
        {
            using var testContext = this.CreateTestContext();

            var code = $@"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestAttribute( Field = 5 )]";
            var compilation = testContext.CreateCompilationModel( code );

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

            var attribute = compilation.Attributes.Single();
            DiagnosticList diagnosticList = new();

            if ( !loader.AttributeDeserializer.TryCreateAttribute( attribute, diagnosticList, out var deserializedAttribute ) )
            {
                throw new AssertionFailedException();
            }

            var value = deserializedAttribute.GetType().GetField( "Field" ).AssertNotNull().GetValue( deserializedAttribute );
            Assert.Equal( 5, value );
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
                    "Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestEnum.A" ) );

            Assert.Equal(
                TestEnum.A,
                this.GetDeserializedProperty(
                    nameof(TestAttribute.ObjectProperty),
                    "Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestEnum.A" ) );

            Assert.Equal(
                new[] { TestEnum.A },
                this.GetDeserializedProperty(
                    nameof(TestAttribute.EnumArrayProperty),
                    "new[]{Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestEnum.A}" ) );
        }

        [Fact]
        public void TestTypes()
        {
            Assert.Equal( typeof(int), this.GetDeserializedProperty( nameof(TestAttribute.TypeProperty), "typeof(int)" ) );

            Assert.Equal(
                typeof(TestEnum),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestEnum)" ) );

            Assert.Null( this.GetDeserializedProperty( nameof(TestAttribute.TypeProperty), "null" ) );

            Assert.Equal(
                new[] { typeof(TestEnum), typeof(int) },
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeArrayProperty),
                    "new[]{typeof(Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestEnum),typeof(int)}" ) );

            Assert.Equal(
                new[] { typeof(TestEnum), typeof(int) },
                this.GetDeserializedProperty(
                    nameof(TestAttribute.ObjectArrayProperty),
                    "new[]{typeof(Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestEnum),typeof(int)}" ) );

            Assert.Null( this.GetDeserializedProperty( nameof(TestAttribute.ObjectArrayProperty), "null" ) );

            Assert.Equal(
                new[] { typeof(TestEnum), null },
                this.GetDeserializedProperty(
                    nameof(TestAttribute.ObjectArrayProperty),
                    "new[]{typeof(Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestEnum),null}" ) );
        }

        [Fact]
        public void TestRunTimeTypes()
        {
            Assert.IsType<CompileTimeType>(
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(RunTimeEnum)" ) );

            Assert.IsType<CompileTimeType>(
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(RunTimeEnum[])" ) );

            Assert.IsType<CompileTimeType>(
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(System.Collections.Generic.List<RunTimeEnum>)" ) );

            Assert.IsType<CompileTimeType>(
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(GenericRunTimeType<int>)" ) );

            Assert.IsType<CompileTimeType>(
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(GenericStruct*)" ) );

            var dependentCode = "public class MyExternClass {} public enum MyExternEnum { A, B }";
            var typeValue = this.GetDeserializedProperty( nameof(TestAttribute.TypeProperty), "typeof(MyExternClass)", dependentCode );
            Assert.Equal( "MyExternClass", Assert.IsType<CompileTimeType>( typeValue ).FullName );

            // When assigning to a run-time-only enum, the enum primitive value is used. 
            var objectValue = this.GetDeserializedProperty( nameof(TestAttribute.ObjectProperty), "MyExternEnum.B", dependentCode );
            Assert.Equal( 1, objectValue );
        }

        [Fact]
        public void TestArrayType()
        {
            Assert.Equal(
                typeof(int[]),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(int[])" ) );

            Assert.Equal(
                typeof(int[,]),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(int[,])" ) );
        }

        [Fact]
        public void TestPointerType()
        {
            Assert.Equal(
                typeof(int*),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(int*)" ) );

            Assert.Equal(
                typeof(int*[]),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(int*[])" ) );
        }

        [Fact]
        public void TestGenericType()
        {
            Assert.Equal(
                typeof(List<int>),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(System.Collections.Generic.List<int>)" ) );

            Assert.Equal(
                typeof(List<>),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(System.Collections.Generic.List<>)" ) );

            Assert.Equal(
                typeof(List<List<int>>),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(System.Collections.Generic.List<System.Collections.Generic.List<int>>)" ) );

            Assert.Equal(
                typeof(List<int[]>),
                this.GetDeserializedProperty(
                    nameof(TestAttribute.TypeProperty),
                    "typeof(System.Collections.Generic.List<int[]>)" ) );
        }

        [Fact]
        public void TestParams()
        {
            object Deserialize( string args )
            {
                using var testContext = this.CreateTestContext();

                var code = $@"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestParamsAttribute( {args} )]";
                var compilation = testContext.CreateCompilationModel( code );

                using UnloadableCompileTimeDomain domain = new();
                var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

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

        [Fact]
        public void AttributesOfUnknownTypeAreIgnored()
        {
            using var testContext = this.CreateTestContext();

            var code = @"[assembly: UnknownType] [UnknownType] class C {}";
            var compilation = testContext.CreateCompilationModel( code, ignoreErrors: true );

            Assert.Empty( compilation.Attributes );
            Assert.Empty( compilation.Types.Single().Attributes );
        }

        [Fact]
        public void PropertiesOfInvalidNameAreIgnored()
        {
            using var testContext = this.CreateTestContext();

            var code = $@"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestAttribute( InvalidProperty = 0 )]";
            var compilation = testContext.CreateCompilationModel( code, ignoreErrors: true );

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

            var attribute = compilation.Attributes.Single();
            DiagnosticList diagnosticList = new();

            Assert.True( loader.AttributeDeserializer.TryCreateAttribute( attribute, diagnosticList, out _ ) );
            Assert.Empty( diagnosticList );
        }

        [Fact]
        public void AttributesWithMissingConstructorAreIgnored()
        {
            using var testContext = this.CreateTestContext();

            var code = $@"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestAttribute( 0 )] "
                       + "[Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestAttribute( 0 )] class C {}";

            var compilation = testContext.CreateCompilationModel( code, ignoreErrors: true );

            Assert.Empty( compilation.Attributes );
            Assert.Empty( compilation.Types.Single().Attributes );
        }

        [Fact]
        public void PropertiesWithInvalidValueAreIgnored()
        {
            using var testContext = this.CreateTestContext();

            var code = @"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestAttribute( Int32Property = ""a"" )]";
            var compilation = testContext.CreateCompilationModel( code, ignoreErrors: true );

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

            var attribute = compilation.Attributes.Single();
            DiagnosticList diagnosticList = new();

            Assert.True( loader.AttributeDeserializer.TryCreateAttribute( attribute, diagnosticList, out _ ) );
            Assert.Empty( diagnosticList );
        }

        [Fact]
        public void AttributesWithInvalidConstructorArgumentsAreIgnored()
        {
            using var testContext = this.CreateTestContext();

            var code = @"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.TestAttribute( 0 )]";
            var compilation = testContext.CreateCompilationModel( code, ignoreErrors: true );

            Assert.Empty( compilation.Attributes );
        }

        [Fact]
        public void ThrowingConstructorFailsSafely()
        {
            using var testContext = this.CreateTestContext();

            var code = @"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.ThrowingAttribute( true )]";
            var compilation = testContext.CreateCompilationModel( code );
            var attribute = compilation.Attributes.Single();

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

            DiagnosticList diagnosticList = new();

            Assert.False( loader.AttributeDeserializer.TryCreateAttribute( attribute, diagnosticList, out var deserializedAttribute ) );
            Assert.Contains( diagnosticList, d => d.Id == GeneralDiagnosticDescriptors.ExceptionInUserCodeWithoutTarget.Id );
            Assert.Null( deserializedAttribute );
        }

        [Fact]
        public void ThrowingPropertyFailsSafely()
        {
            using var testContext = this.CreateTestContext();

            var code = @"[assembly: Metalama.Framework.Tests.UnitTests.CompileTime.AttributeDeserializerTests.ThrowingAttribute( false, Property = 0 )]";
            var compilation = testContext.CreateCompilationModel( code );
            var attribute = compilation.Attributes.Single();

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

            DiagnosticList diagnosticList = new();

            Assert.False( loader.AttributeDeserializer.TryCreateAttribute( attribute, diagnosticList, out var deserializedAttribute ) );
            Assert.Contains( diagnosticList, d => d.Id == GeneralDiagnosticDescriptors.ExceptionInUserCodeWithoutTarget.Id );
            Assert.Null( deserializedAttribute );
        }

        [Fact]
        public void RunTimeOnlyAttributeType()
        {
            using var testContext = this.CreateTestContext();

            var code = @"[assembly: MyAttribute] class MyAttribute : System.Attribute {}";

            var compilation = testContext.CreateCompilationModel( code );
            var attribute = compilation.Attributes.Single();

            using UnloadableCompileTimeDomain domain = new();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );

            DiagnosticList diagnosticList = new();

            Assert.False( loader.AttributeDeserializer.TryCreateAttribute( attribute, diagnosticList, out _ ) );
            Assert.Contains( diagnosticList, d => d.Id == AttributeDeserializerDiagnostics.CannotFindAttributeType.Id );
        }

        // ReSharper disable UnusedParameter.Local
#pragma warning disable SA1401
#pragma warning disable CA1822

        public enum TestEnum
        {
            A,
            B
        }

        public class TestAttribute : Attribute
        {
            public TestAttribute() { }

            public TestAttribute( string s ) { }

            public int Int32Property { get; set; }

            public string? StringProperty { get; set; }

            public object? ObjectProperty { get; set; }

            public TestEnum EnumProperty { get; set; }

            public Type? TypeProperty { get; set; }

            public int[]? Int32ArrayProperty { get; set; }

            public object[]? ObjectArrayProperty { get; set; }

            public Type[]? TypeArrayProperty { get; set; }

            public TestEnum[]? EnumArrayProperty { get; set; }

            public int Field;
        }

        public class TestParamsAttribute : Attribute
        {
            public object Value { get; private set; }

            public TestParamsAttribute( params string[] p ) { this.Value = p; }

            public TestParamsAttribute( params int[] p ) { this.Value = p; }

            public TestParamsAttribute( params Type[] p ) { this.Value = p; }

            public TestParamsAttribute( params object[] p ) { this.Value = p; }
        }

        public class ThrowingAttribute : Attribute
        {
            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            public ThrowingAttribute( bool throws )
            {
                if ( throws )
                {
                    throw new InvalidOperationException();
                }
            }

            public int Property
            {
                get => throw new InvalidOperationException();
                set => throw new InvalidOperationException();
            }
        }

        private class HackedSystemTypeResolver : SystemTypeResolver
        {
            public HackedSystemTypeResolver( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

            protected override bool IsStandardAssemblyName( string assemblyName )
                => base.IsStandardAssemblyName( assemblyName ) || assemblyName == this.GetType().Assembly.GetName().Name;
        }
    }
}