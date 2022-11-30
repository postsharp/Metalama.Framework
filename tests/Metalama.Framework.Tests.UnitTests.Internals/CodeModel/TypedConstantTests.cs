// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Testing.Api;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class TypedConstantTests : UnitTestSuite
    {
        [Fact]
        public void Unassigned()
        {
            TypedConstant c = default;
            Assert.False( c.IsInitialized );
            Assert.Throws<ArgumentNullException>( () => c.Type );
            Assert.Throws<ArgumentNullException>( () => c.Value );
            Assert.Throws<ArgumentNullException>( () => c.IsNullOrDefault );
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
        [InlineData( typeof(int) )]
        [InlineData( ConsoleColor.Blue )]
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

            _ = TypedConstant.Create( value );
        }
    }
}