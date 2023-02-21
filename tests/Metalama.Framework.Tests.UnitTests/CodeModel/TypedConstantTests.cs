// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Testing.UnitTesting;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public sealed class TypedConstantTests : UnitTestClass
    {
        [Fact]
        public void Unassigned()
        {
            TypedConstant c = default;
            Assert.False( c.IsInitialized );
            Assert.Throws<InvalidOperationException>( () => c.Type );
            Assert.Throws<InvalidOperationException>( () => c.Value );
            Assert.Throws<InvalidOperationException>( () => c.IsNullOrDefault );
        }

        [Fact]
        public void Assigned()
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );
            var c = TypedConstant.Create( 1, emptyCompilation.Factory.GetSpecialType( SpecialType.Int32 ) );
            Assert.True( c.IsInitialized );
            Assert.NotNull( c.Type );
            Assert.NotNull( c.Value );
            Assert.False( c.IsNullOrDefault );
        }

        [Fact]
        public void Null()
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );
            var c = TypedConstant.Create( null, emptyCompilation.Factory.GetSpecialType( SpecialType.String ) );
            Assert.True( c.IsInitialized );
            Assert.NotNull( c.Type );
            Assert.Null( c.Value );
            Assert.True( c.IsNullOrDefault );
        }

#pragma warning disable SA1139
        [Theory]
        [InlineData( (byte) 1 )]
        [InlineData( (sbyte) 1 )]
        [InlineData( (short) 1 )]
        [InlineData( (ushort) 1 )]
        [InlineData( 1 )]
        [InlineData( (uint) 1 )]
        [InlineData( (long) 1 )]
        [InlineData( (ulong) 1 )]
        [InlineData( "" )]
        [InlineData( new[] { (byte) 1, (byte) 2 } )]
        [InlineData( new[] { (sbyte) 1 } )]
        [InlineData( new[] { (short) 1 } )]
        [InlineData( new[] { (ushort) 1 } )]
        [InlineData( new[] { 1 } )]
        [InlineData( new[] { (uint) 1 } )]
        [InlineData( new[] { (long) 1 } )]
        [InlineData( new[] { (ulong) 1 } )]
        [InlineData( new object[] { new[] { "" } } )]
        [InlineData( new object[] { new[] { typeof(int) } } )]
        [InlineData( new[] { ConsoleColor.Blue } )]
#pragma warning restore SA1139
        public void CreateFromValue( object value )
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );

            using var userCodeContext = UserCodeExecutionContext.WithContext( testContext.ServiceProvider, emptyCompilation );

            var c = TypedConstant.Create( value );

            Assert.Equal( c.Value, value );
            Assert.Equal( c.Type, emptyCompilation.Factory.GetTypeByReflectionType( value.GetType() ) );
        }

        [Fact]
        public void CreateFromValueEnum()
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );

            using var userCodeContext = UserCodeExecutionContext.WithContext( testContext.ServiceProvider, emptyCompilation );

            var c = TypedConstant.Create( ConsoleColor.Blue );

            Assert.Equal( c.Value, (int) ConsoleColor.Blue );
            Assert.Equal( c.Type, emptyCompilation.Factory.GetTypeByReflectionType( typeof(ConsoleColor) ) );
        }

        [Fact]
        public void CreateFromValueType()
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );

            using var userCodeContext = UserCodeExecutionContext.WithContext( testContext.ServiceProvider, emptyCompilation );

            var c = TypedConstant.Create( typeof(int) );

            Assert.Equal( c.Value, emptyCompilation.Factory.GetTypeByReflectionType( typeof(int) ) );
            Assert.Equal( c.Type, emptyCompilation.Factory.GetTypeByReflectionType( typeof(Type) ) );
        }

#pragma warning disable SA1139
        [Theory]
        [InlineData( (byte) 1, typeof(byte) )]
        [InlineData( (sbyte) 1, typeof(sbyte) )]
        [InlineData( (short) 1, typeof(short) )]
        [InlineData( (ushort) 1, typeof(ushort) )]
        [InlineData( 1, typeof(int) )]
        [InlineData( (uint) 1, typeof(uint) )]
        [InlineData( (long) 1, typeof(long) )]
        [InlineData( (ulong) 1, typeof(ulong) )]
        [InlineData( "", typeof(string) )]
#pragma warning restore SA1139
        public void CreateFromValueTyped( object value, Type type )
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );

            using var userCodeContext = UserCodeExecutionContext.WithContext( testContext.ServiceProvider, emptyCompilation );

            var c = TypedConstant.Create( value, type );

            Assert.Equal( c.Value, value );
            Assert.Equal( c.Type, emptyCompilation.Factory.GetTypeByReflectionType( type ) );
        }

#pragma warning disable SA1139
        [Theory]
        [InlineData( (byte) 1, typeof(byte) )]
        [InlineData( (sbyte) 1, typeof(sbyte) )]
        [InlineData( (short) 1, typeof(short) )]
        [InlineData( (ushort) 1, typeof(ushort) )]
        [InlineData( 1, typeof(int) )]
        [InlineData( (uint) 1, typeof(uint) )]
        [InlineData( (long) 1, typeof(long) )]
        [InlineData( (ulong) 1, typeof(ulong) )]
        [InlineData( "", typeof(string) )]
#pragma warning restore SA1139
        public void CreateFromValueArray( object value, Type type )
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );

            using var userCodeContext = UserCodeExecutionContext.WithContext( testContext.ServiceProvider, emptyCompilation );

            var array = Array.CreateInstance( type, 1 );
            array.SetValue( value, 0 );
            var c = TypedConstant.Create( array, array.GetType() );

            Assert.Equal( c.Value, array );
            Assert.Equal( c.Type, emptyCompilation.Factory.GetTypeByReflectionType( array.GetType() ) );
        }

#pragma warning disable SA1139
        [Theory]
        [InlineData( (byte) 1, typeof(byte?) )]
        [InlineData( (sbyte) 1, typeof(sbyte?) )]
        [InlineData( (short) 1, typeof(short?) )]
        [InlineData( (ushort) 1, typeof(ushort?) )]
        [InlineData( 1, typeof(int?) )]
        [InlineData( (uint) 1, typeof(uint?) )]
        [InlineData( (long) 1, typeof(long?) )]
        [InlineData( (ulong) 1, typeof(ulong?) )]
#pragma warning restore SA1139
        public void CreateFromValueTypedNullable( object value, Type type )
        {
            using var testContext = this.CreateTestContext();

            var emptyCompilation = testContext.CreateCompilationModel( "" );

            using var userCodeContext = UserCodeExecutionContext.WithContext( testContext.ServiceProvider, emptyCompilation );

            var c = TypedConstant.Create( value, type );

            Assert.Equal( c.Value, value );
            Assert.Equal( c.Type, emptyCompilation.Factory.GetTypeByReflectionType( type ) );
        }
    }
}