// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.ReflectionMocks;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{

    public class AttributeDeserializerTests : TestBase
    {

        private static object? GetDeserializedProperty( string property, string value, string? dependentCode = null )
        {
            var code = $@"[assembly: Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestAttribute( {property} = {value} )]";
            var compilation = CreateCompilation( code, dependentCode: dependentCode );
            AttributeDeserializer deserializer = new( new SystemTypeResolver() );
            var attribute = compilation.Attributes.Single();
            var deserializedAttribute = deserializer.CreateAttribute( attribute );
            return deserializedAttribute.GetType().GetProperty( property ).AssertNotNull().GetValue( deserializedAttribute );
        }

        [Fact]
        public void TestPrimitiveTypes()
        {
            Assert.Equal( 5, GetDeserializedProperty( nameof( TestAttribute.Int32Property ), "5" ) );
            Assert.Equal( "Zuzana", GetDeserializedProperty( nameof( TestAttribute.StringProperty ), "\"Zuzana\"" ) );
            Assert.Equal( new[] { 5 }, GetDeserializedProperty( nameof( TestAttribute.Int32ArrayProperty ), "new[]{5}" ) );
        }

        [Fact]
        public void TestBoxedTypes()
        {
            Assert.Equal( 5, GetDeserializedProperty( nameof( TestAttribute.ObjectProperty ), "5" ) );
            Assert.Equal( "Zuzana", GetDeserializedProperty( nameof( TestAttribute.ObjectProperty ), "\"Zuzana\"" ) );
            Assert.Equal( new[] { 5 }, GetDeserializedProperty( nameof( TestAttribute.ObjectProperty ), "new[]{5}" ) );
        }

        [Fact]
        public void TestEnums()
        {
            Assert.Equal( TestEnum.A, GetDeserializedProperty( nameof( TestAttribute.EnumProperty ), "Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum.A" ) );
            Assert.Equal( TestEnum.A, GetDeserializedProperty( nameof( TestAttribute.ObjectProperty ), "Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum.A" ) );
            Assert.Equal( new[] { TestEnum.A }, GetDeserializedProperty( nameof( TestAttribute.EnumArrayProperty ), "new[]{Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum.A}" ) );
        }

        [Fact]
        public void TestTypes()
        {
            Assert.Equal( typeof( int ), GetDeserializedProperty( nameof( TestAttribute.TypeProperty ), "typeof(int)" ) );
            Assert.Equal( typeof( TestEnum ), GetDeserializedProperty( nameof( TestAttribute.TypeProperty ), "typeof(Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum)" ) );
            Assert.Equal( new[] { typeof( TestEnum ), typeof( int ) }, GetDeserializedProperty( nameof( TestAttribute.TypeArrayProperty ), "new[]{typeof(Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum),typeof(int)}" ) );
            Assert.Equal( new[] { typeof( TestEnum ), typeof( int ) }, GetDeserializedProperty( nameof( TestAttribute.ObjectArrayProperty ), "new[]{typeof(Caravela.Framework.Tests.UnitTests.AttributeDeserializerTests.TestEnum),typeof(int)}" ) );
        }

        [Fact]
        public void TestNonRunTimeType()
        {
            var dependentCode = "public class MyExternClass {} public enum MyExternEnum { A, B }";
            var typeValue = GetDeserializedProperty( nameof( TestAttribute.TypeProperty ), "typeof(MyExternClass)", dependentCode );
            Assert.Equal( "MyExternClass", Assert.IsType<CompileTimeType>( typeValue ).FullName );

            var objectValue = GetDeserializedProperty( nameof( TestAttribute.ObjectProperty ), "MyExternEnum.B", dependentCode );
            Assert.Equal( 1, objectValue );
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
    }
}