// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Serialization;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public class IntrinsicSerializationTests : SerializationTestsBase
    {
        [Fact]
        public void TestInt32_0()
        {
            TestSerialization( 0 );
        }

        [Fact]
        public void TestInt32_1()
        {
            TestSerialization( 1 );
        }

        [Fact]
        public void TestInt32_m1()
        {
            TestSerialization( -1 );
        }

        [Fact]
        public void TestInt32_1000()
        {
            TestSerialization( 1000 );
        }

        [Fact]
        public void TestInt32_m1000()
        {
            TestSerialization( -1000 );
        }

        [Fact]
        public void TestInt32_100000()
        {
            TestSerialization( 100000 );
        }

        [Fact]
        public void TestInt32_m100000()
        {
            TestSerialization( -100000 );
        }

        [Fact]
        public void TestInt32_10000000()
        {
            TestSerialization( 10000000 );
        }

        [Fact]
        public void TestInt32_m10000000()
        {
            TestSerialization( -10000000 );
        }

        [Fact]
        public void TestInt32_Max()
        {
            TestSerialization( int.MaxValue );
        }

        [Fact]
        public void TestInt32_Min()
        {
            TestSerialization( int.MinValue );
        }

        [Fact]
        public void TestString_SimpleWords()
        {
            TestSerialization( "SimpleWords" );
        }

        [Fact]
        public void TestDottedString_SimpleDottedWords()
        {
            TestSerialization( (DottedString) "Simple.Dotted.Words" );
            TestSerialization( new DottedString[] { "A", "A.B", "A.B.C", "A", "A.B" } );
        }

        [Fact]
        public void TestDottedString_DottedWordsWithReservedNames()
        {
            TestSerialization( (DottedString) "Simple.Dotted.Words, mscorlib" );
        }

        [Fact]
        public void TestDottedString_NullAndEmpty()
        {
            TestSerialization( (DottedString?) null );
            TestSerialization( (DottedString) "" );
        }

        [Fact]
        public void TestStruct_DateTime()
        {
            TestSerialization( DateTime.Now );
        }

        [Fact]
        public void TestBoolean_True()
        {
            TestSerialization( true );
        }

        [Fact]
        public void TestBoolean_False()
        {
            TestSerialization( false );
        }

        [Fact]
        public void TestBoxedBoolean_False()
        {
            TestSerialization( (object) false );
        }

        [Fact]
        public void TestByte()
        {
            TestSerialization( (byte) 0 );
            TestSerialization( (byte) 1 );
            TestSerialization( (byte) 255 );
        }

        [Fact]
        public void TestChar()
        {
            TestSerialization( 'a' );
            TestSerialization( (char) 0 );
            TestSerialization( (char) 255 );
            TestSerialization( (char) 65511 );
        }

        [Fact]
        public void TestDateTime()
        {
            TestSerialization( DateTime.Now );
            TestSerialization( DateTime.MinValue );
            TestSerialization( DateTime.MaxValue );
        }

        [Fact]
        public void TestDecimal()
        {
            TestSerialization( decimal.Zero );
            TestSerialization( -99999999m );
            TestSerialization( 999999m );
            TestSerialization( decimal.MaxValue );
            TestSerialization( decimal.MinValue );
            TestSerialization( decimal.One );
            TestSerialization( decimal.MinusOne );
        }

        [Fact]
        public void TestString()
        {
            TestSerialization( (string?) null );
            TestSerialization( "test" );
            TestSerialization( string.Empty );
        }

        [Fact]
        public void TestDouble()
        {
            TestSerialization( 0.0 );
            TestSerialization( -1.0 );
            TestSerialization( 1.0 );
            TestSerialization( double.MinValue );
            TestSerialization( double.MaxValue );
            TestSerialization( double.NaN );
            TestSerialization( double.NegativeInfinity );
            TestSerialization( double.PositiveInfinity );
        }

        [Fact]
        public void TestSingle()
        {
            TestSerialization( 0.0f );
            TestSerialization( -1.0f );
            TestSerialization( 1.0f );
            TestSerialization( float.MinValue );
            TestSerialization( float.MaxValue );
            TestSerialization( float.NaN );
            TestSerialization( float.NegativeInfinity );
            TestSerialization( float.PositiveInfinity );
        }

        [Fact]
        public void TestGuid()
        {
            TestSerialization( Guid.Empty );
            TestSerialization( Guid.NewGuid() );
        }

        [Fact]
        public void TestInt16()
        {
            TestSerialization( short.MinValue );
            TestSerialization( short.MaxValue );
            TestSerialization( (short) 0 );
        }

        [Fact]
        public void TestInt64_223372036854775807()
        {
            TestSerialization( 223372036854775807 );
        }

        [Fact]
        public void TestInt64_m223372036854775807()
        {
            TestSerialization( -223372036854775807 );
        }

        [Fact]
        public void TestInt64()
        {
            TestSerialization( long.MinValue );
            TestSerialization( long.MaxValue );
            TestSerialization( 0L );
        }

        [Fact]
        public void TestUInt16()
        {
            TestSerialization( ushort.MinValue );
            TestSerialization( ushort.MaxValue );
            TestSerialization( (ushort) 0 );
        }

        [Fact]
        public void TestUInt64()
        {
            TestSerialization( ulong.MinValue );
            TestSerialization( ulong.MaxValue );
            TestSerialization( 0UL );
        }

        [Fact]
        public void TestUInt32()
        {
            TestSerialization( uint.MinValue );
            TestSerialization( uint.MaxValue );
            TestSerialization( 0U );
        }

        [Fact]
        public void TestNullableInt()
        {
            TestSerialization( (int?) 0 );
            TestSerialization( (int?) int.MaxValue );
            TestSerialization( (int?) int.MinValue );
            TestSerialization( (int?) null );
        }

        [Fact]
        public void TestNullableEnum()
        {
            TestSerialization( (TypeCode?) TypeCode.Boolean );
            TestSerialization( (TypeCode?) null );
        }

        [Fact]
        public void TestArray_Ints()
        {
            TestSerialization( new[] { 1, int.MinValue, 9, int.MaxValue } );
        }

        [Fact]
        public void TestArray_Objects()
        {
            TestSerialization( new[] { new SimpleType(), new SimpleType(), new SimpleType(), new SimpleType() } );
        }

        [Fact]
        public void TestArray_Structs()
        {
            TestSerialization( new[] { DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue } );
        }

#if !SILVERLIGHT
        [Fact]
        public void TestArray_WithLowerBoundNEqZero()
        {
            var array = Array.CreateInstance( typeof(int), new[] { 5 }, new[] { 5 } ); // int[5..10]

            array.SetValue( 0, 5 );
            array.SetValue( 1, 6 );
            array.SetValue( 2, 7 );
            array.SetValue( 3, 8 );
            array.SetValue( 4, 9 );

            TestSerialization( array );
        }
#endif

        // [Fact]
        //        public void TestArray_Generics()
        //        {
        //            TestValue( new[] {new Nullable<int>(4), new Nullable<int>(int.MinValue), new Nullable<int>(int.MaxValue)} );
        //        }

        [Fact]
        public void TestArray_BoxedStructs()
        {
            TestSerialization( new object[] { DateTime.Now, 11, false, "test", 0.0 } );
        }

        [Fact]
        public void TestArray_WithNulls()
        {
            TestSerialization( new object[2] );
        }

        [Fact]
        public void TestBoxedIntrinsics()
        {
            TestSerialization( (object) 5 );
            TestSerialization( (object) -13.0 );
            TestSerialization( (object) "test" );
            TestSerialization( (object) DateTime.Now );
        }

        [Serializer( typeof(Serializer) )]
        public class SimpleType : IEquatable<SimpleType>
        {
            public string? Name { get; set; }

            public bool Equals( SimpleType? other )
            {
                if ( ReferenceEquals( null, other ) )
                {
                    return false;
                }

                if ( ReferenceEquals( this, other ) )
                {
                    return true;
                }

                return StringComparer.Ordinal.Equals( this.Name, other.Name );
            }

            public override bool Equals( object? obj )
            {
                if ( ReferenceEquals( null, obj ) )
                {
                    return false;
                }

                if ( ReferenceEquals( this, obj ) )
                {
                    return true;
                }

                if ( obj.GetType() != this.GetType() )
                {
                    return false;
                }

                return this.Equals( (SimpleType) obj );
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                var name = this.Name;

                return name != null ? StringComparer.Ordinal.GetHashCode( name ) : 0;
            }

            public class Serializer : ReferenceTypeSerializer
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new SimpleType();
                }

                public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    initializationArguments.SetValue( "_", ((SimpleType) obj).Name );
                }

                public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
                {
                    ((SimpleType) obj).Name = initializationArguments.GetValue<string>( "_" );
                }
            }
        }
    }
}