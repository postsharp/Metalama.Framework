// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization
{
    public class PrimitiveSerializersTests : SerializerTestsBase
    {
        [Fact]
        public void TestInt()
        {
            Assert.Equal( "42", this.Serialize( 42 ).ToString() );
            Assert.Equal( "2147483647", this.Serialize( int.MaxValue ).ToString() );
            Assert.Equal( "-2147483648", this.Serialize( int.MinValue ).ToString() );
        }

        [Fact]
        public void TestChar()
        {
            Assert.Equal( "'\\0'", this.Serialize( '\0' ).ToString() );
            Assert.Equal( "'\\n'", this.Serialize( '\n' ).ToString() );
            Assert.Equal( "'A'", this.Serialize( 'A' ).ToString() );
            Assert.Equal( "'ř'", this.Serialize( 'ř' ).ToString() );
            Assert.Equal( "'衛'", this.Serialize( '衛' ).ToString() );
        }

        [Fact]
        public void TestBool()
        {
            Assert.Equal( "true", this.Serialize( true ).ToString() );
            Assert.Equal( "false", this.Serialize( false ).ToString() );
        }

        [Fact]
        public void TestByte()
        {
            Assert.Equal( "42", this.Serialize<byte>( 42 ).ToString() );
            Assert.Equal( "0", this.Serialize<byte>( 0 ).ToString() );
            Assert.Equal( "255", this.Serialize<byte>( 255 ).ToString() );
        }

        [Fact]
        public void TestSByte()
        {
            Assert.Equal( "42", this.Serialize<sbyte>( 42 ).ToString() );
            Assert.Equal( "127", this.Serialize<sbyte>( 127 ).ToString() );
            Assert.Equal( "-128", this.Serialize<sbyte>( -128 ).ToString() );
        }

        [Fact]
        public void TestUShort()
        {
            Assert.Equal( "42", this.Serialize<ushort>( 42 ).ToString() );
            Assert.Equal( ushort.MaxValue.ToString(), this.Serialize( ushort.MaxValue ).ToString() );
            Assert.Equal( ushort.MinValue.ToString(), this.Serialize( ushort.MinValue ).ToString() );
        }

        [Fact]
        public void TestShort()
        {
            Assert.Equal( "42", this.Serialize<short>( 42 ).ToString() );
            Assert.Equal( short.MaxValue.ToString(), this.Serialize( short.MaxValue ).ToString() );
            Assert.Equal( short.MinValue.ToString(), this.Serialize( short.MinValue ).ToString() );
        }

        [Fact]
        public void TestULong()
        {
            Assert.Equal( "42UL", this.Serialize<ulong>( 42 ).ToString() );
            Assert.Equal( ulong.MaxValue + "UL", this.Serialize( ulong.MaxValue ).ToString() );
            Assert.Equal( ulong.MinValue + "UL", this.Serialize( ulong.MinValue ).ToString() );
        }

        [Fact]
        public void TestUInt()
        {
            Assert.Equal( "42U", this.Serialize<uint>( 42 ).ToString() );
            Assert.Equal( uint.MaxValue + "U", this.Serialize( uint.MaxValue ).ToString() );
            Assert.Equal( uint.MinValue + "U", this.Serialize( uint.MinValue ).ToString() );
        }

        [Fact]
        public void TestLong()
        {
            Assert.Equal( "42L", this.Serialize<long>( 42 ).ToString() );
            Assert.Equal( "9223372036854775807L", this.Serialize( long.MaxValue ).ToString() );
            Assert.Equal( "-9223372036854775808L", this.Serialize( long.MinValue ).ToString() );
        }

        [Fact]
        public void TestFloat()
        {
            Assert.Equal( "42F", this.Serialize<float>( 42 ).ToString() );
            Assert.Equal( "3.1415927F", this.Serialize( 3.1415927F ).ToString() );
            Assert.Equal( "-3.402823E+38F", this.Serialize( -3.402823E+38F ).ToString() );
            Assert.Equal( "3.402823E+38F", this.Serialize( 3.402823E+38F ).ToString() );
            Assert.Equal( "1E-45F", this.Serialize( float.Epsilon ).ToString() );
            Assert.Equal( "float.PositiveInfinity", this.Serialize( float.PositiveInfinity ).ToString() );
            Assert.Equal( "float.NegativeInfinity", this.Serialize( float.NegativeInfinity ).ToString() );
            Assert.Equal( "float.NaN", this.Serialize( float.NaN ).ToString() );
            Assert.Equal( "0F", this.Serialize<float>( 0 ).ToString() );
            Assert.Equal( "0F", this.Serialize<float>( -0 ).ToString() );
        }

        [Fact]
        public void TestDouble()
        {
            Assert.Equal( "42", this.Serialize<double>( 42 ).ToString() );
            Assert.Equal( "3.14159285", this.Serialize( 3.14159285 ).ToString() );
            Assert.Equal( "-3.402823E+38", this.Serialize( -3.402823E+38 ).ToString() );
            Assert.Equal( "3.402823E+38", this.Serialize( 3.402823E+38 ).ToString() );
            Assert.Equal( "5E-324", this.Serialize( double.Epsilon ).ToString() );
            Assert.Equal( "double.PositiveInfinity", this.Serialize( double.PositiveInfinity ).ToString() );
            Assert.Equal( "double.NegativeInfinity", this.Serialize( double.NegativeInfinity ).ToString() );
            Assert.Equal( "double.NaN", this.Serialize( double.NaN ).ToString() );
            Assert.Equal( "0", this.Serialize<double>( 0 ).ToString() );
            Assert.Equal( "0", this.Serialize<double>( -0 ).ToString() );
        }

        [Fact]
        public void TestDecimal()
        {
            Assert.Equal( "42M", this.Serialize<decimal>( 42 ).ToString() );
            Assert.Equal( "1.0M", this.Serialize( 1.0M ).ToString() );
            Assert.Equal( "1.00M", this.Serialize( 1.00M ).ToString() );
            Assert.Equal( "4.2M", this.Serialize( 4.2M ).ToString() );
            Assert.Equal( "-340282300M", this.Serialize( -3.402823E+8M ).ToString() );
            Assert.Equal( "340282300M", this.Serialize( 3.402823E+8M ).ToString() );
            Assert.Equal( "-79228162514264337593543950335M", this.Serialize( decimal.MinValue ).ToString() );
            Assert.Equal( "79228162514264337593543950335M", this.Serialize( decimal.MaxValue ).ToString() );
            Assert.Equal( "0M", this.Serialize<decimal>( 0 ).ToString() );
        }

        [Fact]
        public unsafe void TestUIntPtr()
        {
            var pointer = (int*) 300;
            Assert.Equal( "new global::System.UIntPtr(400UL)", this.Serialize( new UIntPtr( 400UL ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new global::System.UIntPtr(200UL)", this.Serialize( new UIntPtr( 200 ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new global::System.UIntPtr(300UL)", this.Serialize( new UIntPtr( pointer ) ).NormalizeWhitespace().ToString() );
        }

        [Fact]
        public unsafe void TestIntPtr()
        {
            var pointer = (int*) 300;
            Assert.Equal( "new global::System.IntPtr(-400L)", this.Serialize( new IntPtr( -400L ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new global::System.IntPtr(-200L)", this.Serialize( new IntPtr( -200 ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new global::System.IntPtr(300L)", this.Serialize( new IntPtr( pointer ) ).NormalizeWhitespace().ToString() );
        }
    }
}