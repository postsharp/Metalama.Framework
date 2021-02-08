using System;
using Caravela.Framework.Impl.Templating.Serialization;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization
{
    public class PrimitiveSerializersTests
    {
        [Fact]
        public void TestInt()
        {
            var serializer = new IntSerializer();
            Assert.Equal( "42", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( "2147483647", serializer.Serialize( int.MaxValue ).ToString() );
            Assert.Equal( "-2147483648", serializer.Serialize( int.MinValue ).ToString() );
        }

        [Fact]
        public void TestChar()
        {
            var serializer = new CharSerializer();
            Assert.Equal( "'\\0'", serializer.Serialize( '\0' ).ToString() );
            Assert.Equal( "'\\n'", serializer.Serialize( '\n' ).ToString() );
            Assert.Equal( "'A'", serializer.Serialize( 'A' ).ToString() );
            Assert.Equal( "'ř'", serializer.Serialize( 'ř' ).ToString() );
            Assert.Equal( "'衛'", serializer.Serialize( '衛' ).ToString() );
        }

        [Fact]
        public void TestBool()
        {
            var serializer = new BoolSerializer();
            Assert.Equal( "true", serializer.Serialize( true ).ToString() );
            Assert.Equal( "false", serializer.Serialize( false ).ToString() );
        }

        [Fact]
        public void TestByte()
        {
            var serializer = new ByteSerializer();
            Assert.Equal( "42", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( "0", serializer.Serialize( 0 ).ToString() );
            Assert.Equal( "255", serializer.Serialize( 255 ).ToString() );
        }

        [Fact]
        public void TestSByte()
        {
            var serializer = new SByteSerializer();
            Assert.Equal( "42", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( "127", serializer.Serialize( 127 ).ToString() );
            Assert.Equal( "-128", serializer.Serialize( -128 ).ToString() );
        }

        [Fact]
        public void TestUShort()
        {
            var serializer = new UShortSerializer();
            Assert.Equal( "42", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( ushort.MaxValue.ToString(), serializer.Serialize( ushort.MaxValue ).ToString() );
            Assert.Equal( ushort.MinValue.ToString(), serializer.Serialize( ushort.MinValue ).ToString() );
        }

        [Fact]
        public void TestShort()
        {
            var serializer = new ShortSerializer();
            Assert.Equal( "42", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( short.MaxValue.ToString(), serializer.Serialize( short.MaxValue ).ToString() );
            Assert.Equal( short.MinValue.ToString(), serializer.Serialize( short.MinValue ).ToString() );
        }

        [Fact]
        public void TestULong()
        {
            var serializer = new ULongSerializer();
            Assert.Equal( "42UL", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( ulong.MaxValue.ToString() + "UL", serializer.Serialize( ulong.MaxValue ).ToString() );
            Assert.Equal( ulong.MinValue.ToString() + "UL", serializer.Serialize( ulong.MinValue ).ToString() );
        }

        [Fact]
        public void TestUInt()
        {
            var serializer = new UIntSerializer();
            Assert.Equal( "42U", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( uint.MaxValue.ToString() + "U", serializer.Serialize( uint.MaxValue ).ToString() );
            Assert.Equal( uint.MinValue.ToString() + "U", serializer.Serialize( uint.MinValue ).ToString() );
        }

        [Fact]
        public void TestLong()
        {
            var serializer = new LongSerializer();
            Assert.Equal( "42L", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( "9223372036854775807L", serializer.Serialize( long.MaxValue ).ToString() );
            Assert.Equal( "-9223372036854775808L", serializer.Serialize( long.MinValue ).ToString() );
        }

        [Fact]
        public void TestFloat()
        {
            var serializer = new FloatSerializer();
            Assert.Equal( "42F", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( "3.1415927F", serializer.Serialize( 3.1415927F ).ToString() );
            Assert.Equal( "-3.402823E+38F", serializer.Serialize( -3.402823E+38F ).ToString() );
            Assert.Equal( "3.402823E+38F", serializer.Serialize( 3.402823E+38F ).ToString() );
            Assert.Equal( "1E-45F", serializer.Serialize( float.Epsilon ).ToString() );
            Assert.Equal( "float.PositiveInfinity", serializer.Serialize( float.PositiveInfinity ).ToString() );
            Assert.Equal( "float.NegativeInfinity", serializer.Serialize( float.NegativeInfinity ).ToString() );
            Assert.Equal( "float.NaN", serializer.Serialize( float.NaN ).ToString() );
            Assert.Equal( "0F", serializer.Serialize( 0 ).ToString() );
            Assert.Equal( "0F", serializer.Serialize( -0 ).ToString() );
        }

        [Fact]
        public void TestDouble()
        {
            var serializer = new DoubleSerializer();
            Assert.Equal( "42", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( "3.14159285", serializer.Serialize( 3.14159285 ).ToString() );
            Assert.Equal( "-3.402823E+38", serializer.Serialize( -3.402823E+38 ).ToString() );
            Assert.Equal( "3.402823E+38", serializer.Serialize( 3.402823E+38 ).ToString() );
            Assert.Equal( "5E-324", serializer.Serialize( double.Epsilon ).ToString() );
            Assert.Equal( "double.PositiveInfinity", serializer.Serialize( double.PositiveInfinity ).ToString() );
            Assert.Equal( "double.NegativeInfinity", serializer.Serialize( double.NegativeInfinity ).ToString() );
            Assert.Equal( "double.NaN", serializer.Serialize( float.NaN ).ToString() );
            Assert.Equal( "0", serializer.Serialize( 0 ).ToString() );
            Assert.Equal( "0", serializer.Serialize( -0 ).ToString() );
        }

        [Fact]
        public void TestDecimal()
        {
            var serializer = new DecimalSerializer();
            Assert.Equal( "42M", serializer.Serialize( 42 ).ToString() );
            Assert.Equal( "1.0M", serializer.Serialize( 1.0M ).ToString() );
            Assert.Equal( "1.00M", serializer.Serialize( 1.00M ).ToString() );
            Assert.Equal( "4.2M", serializer.Serialize( 4.2M ).ToString() );
            Assert.Equal( "-340282300M", serializer.Serialize( -3.402823E+8M ).ToString() );
            Assert.Equal( "340282300M", serializer.Serialize( 3.402823E+8M ).ToString() );
            Assert.Equal( "-79228162514264337593543950335M", serializer.Serialize( decimal.MinValue ).ToString() );
            Assert.Equal( "79228162514264337593543950335M", serializer.Serialize( decimal.MaxValue ).ToString() );
            Assert.Equal( "0M", serializer.Serialize( 0 ).ToString() );
        }

        [Fact]
        public unsafe void TestUIntPtr()
        {
            var pointer = (int*) 300;
            var serializer = new UIntPtrSerializer();
            Assert.Equal( "new System.UIntPtr(400UL)", serializer.Serialize( new UIntPtr( 400UL ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new System.UIntPtr(200UL)", serializer.Serialize( new UIntPtr( 200 ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new System.UIntPtr(300UL)", serializer.Serialize( new UIntPtr( pointer ) ).NormalizeWhitespace().ToString() );
        }

        [Fact]
        public unsafe void TestIntPtr()
        {
            var pointer = (int*) 300;
            var serializer = new IntPtrSerializer();
            Assert.Equal( "new System.IntPtr(-400L)", serializer.Serialize( new IntPtr( -400L ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new System.IntPtr(-200L)", serializer.Serialize( new IntPtr( -200 ) ).NormalizeWhitespace().ToString() );
            Assert.Equal( "new System.IntPtr(300L)", serializer.Serialize( new IntPtr( pointer ) ).NormalizeWhitespace().ToString() );
        }
    }
}