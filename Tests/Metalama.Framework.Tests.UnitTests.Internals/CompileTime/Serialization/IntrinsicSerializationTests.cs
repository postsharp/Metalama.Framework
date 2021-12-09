// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime.Serialization;
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
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 0 );
After:
            SerializationTestsBase.TestSerialization( 0 );
*/
            TestSerialization( 0 );
        }

        [Fact]
        public void TestInt32_1()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 1 );
After:
            SerializationTestsBase.TestSerialization( 1 );
*/
            TestSerialization( 1 );
        }

        [Fact]
        public void TestInt32_m1()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( -1 );
After:
            SerializationTestsBase.TestSerialization( -1 );
*/
            TestSerialization( -1 );
        }

        [Fact]
        public void TestInt32_1000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 1000 );
After:
            SerializationTestsBase.TestSerialization( 1000 );
*/
            TestSerialization( 1000 );
        }

        [Fact]
        public void TestInt32_m1000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( -1000 );
After:
            SerializationTestsBase.TestSerialization( -1000 );
*/
            TestSerialization( -1000 );
        }

        [Fact]
        public void TestInt32_100000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 100000 );
After:
            SerializationTestsBase.TestSerialization( 100000 );
*/
            TestSerialization( 100000 );
        }

        [Fact]
        public void TestInt32_m100000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( -100000 );
After:
            SerializationTestsBase.TestSerialization( -100000 );
*/
            TestSerialization( -100000 );
        }

        [Fact]
        public void TestInt32_10000000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 10000000 );
After:
            SerializationTestsBase.TestSerialization( 10000000 );
*/
            TestSerialization( 10000000 );
        }

        [Fact]
        public void TestInt32_m10000000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( -10000000 );
After:
            SerializationTestsBase.TestSerialization( -10000000 );
*/
            TestSerialization( -10000000 );
        }

        [Fact]
        public void TestInt32_Max()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( int.MaxValue );
After:
            SerializationTestsBase.TestSerialization( int.MaxValue );
*/
            TestSerialization( int.MaxValue );
        }

        [Fact]
        public void TestInt32_Min()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( int.MinValue );
After:
            SerializationTestsBase.TestSerialization( int.MinValue );
*/
            TestSerialization( int.MinValue );
        }

        [Fact]
        public void TestString_SimpleWords()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( "SimpleWords" );
After:
            SerializationTestsBase.TestSerialization( "SimpleWords" );
*/
            TestSerialization( "SimpleWords" );
        }

        [Fact]
        public void TestDottedString_SimpleDottedWords()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (DottedString) "Simple.Dotted.Words" );
            this.TestSerialization( new DottedString[] { "A", "A.B", "A.B.C", "A", "A.B" } );
After:
            SerializationTestsBase.TestSerialization( (DottedString) "Simple.Dotted.Words" );
            SerializationTestsBase.TestSerialization( new DottedString[] { "A", "A.B", "A.B.C", "A", "A.B" } );
*/
            TestSerialization( (DottedString) "Simple.Dotted.Words" );
            TestSerialization( new DottedString[] { "A", "A.B", "A.B.C", "A", "A.B" } );
        }

        [Fact]
        public void TestDottedString_DottedWordsWithReservedNames()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (DottedString) "Simple.Dotted.Words, mscorlib" );
After:
            SerializationTestsBase.TestSerialization( (DottedString) "Simple.Dotted.Words, mscorlib" );
*/
            TestSerialization( (DottedString) "Simple.Dotted.Words, mscorlib" );
        }

        [Fact]
        public void TestDottedString_NullAndEmpty()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (DottedString?) null );
            this.TestSerialization( (DottedString) "" );
After:
            SerializationTestsBase.TestSerialization( (DottedString?) null );
            SerializationTestsBase.TestSerialization( (DottedString) "" );
*/
            TestSerialization( (DottedString?) null );
            TestSerialization( (DottedString) "" );
        }

        [Fact]
        public void TestStruct_DateTime()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( DateTime.Now );
After:
            SerializationTestsBase.TestSerialization( DateTime.Now );
*/
            TestSerialization( DateTime.Now );
        }

        [Fact]
        public void TestBoolean_True()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( true );
After:
            SerializationTestsBase.TestSerialization( true );
*/
            TestSerialization( true );
        }

        [Fact]
        public void TestBoolean_False()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( false );
After:
            SerializationTestsBase.TestSerialization( false );
*/
            TestSerialization( false );
        }

        [Fact]
        public void TestBoxedBoolean_False()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (object) false );
After:
            SerializationTestsBase.TestSerialization( (object) false );
*/
            TestSerialization( (object) false );
        }

        [Fact]
        public void TestByte()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (byte) 0 );
            this.TestSerialization( (byte) 1 );
            this.TestSerialization( (byte) 255 );
After:
            SerializationTestsBase.TestSerialization( (byte) 0 );
            SerializationTestsBase.TestSerialization( (byte) 1 );
            SerializationTestsBase.TestSerialization( (byte) 255 );
*/
            TestSerialization( (byte) 0 );
            TestSerialization( (byte) 1 );
            TestSerialization( (byte) 255 );
        }

        [Fact]
        public void TestChar()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 'a' );
            this.TestSerialization( (char) 0 );
            this.TestSerialization( (char) 255 );
            this.TestSerialization( (char) 65511 );
After:
            SerializationTestsBase.TestSerialization( 'a' );
            SerializationTestsBase.TestSerialization( (char) 0 );
            SerializationTestsBase.TestSerialization( (char) 255 );
            SerializationTestsBase.TestSerialization( (char) 65511 );
*/
            TestSerialization( 'a' );
            TestSerialization( (char) 0 );
            TestSerialization( (char) 255 );
            TestSerialization( (char) 65511 );
        }

        [Fact]
        public void TestDateTime()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( DateTime.Now );
            this.TestSerialization( DateTime.MinValue );
            this.TestSerialization( DateTime.MaxValue );
After:
            SerializationTestsBase.TestSerialization( DateTime.Now );
            SerializationTestsBase.TestSerialization( DateTime.MinValue );
            SerializationTestsBase.TestSerialization( DateTime.MaxValue );
*/
            TestSerialization( DateTime.Now );
            TestSerialization( DateTime.MinValue );
            TestSerialization( DateTime.MaxValue );
        }

        [Fact]
        public void TestDecimal()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( decimal.Zero );
            this.TestSerialization( -99999999m );
            this.TestSerialization( 999999m );
            this.TestSerialization( decimal.MaxValue );
            this.TestSerialization( decimal.MinValue );
            this.TestSerialization( decimal.One );
            this.TestSerialization( decimal.MinusOne );
After:
            SerializationTestsBase.TestSerialization( decimal.Zero );
            SerializationTestsBase.TestSerialization( -99999999m );
            SerializationTestsBase.TestSerialization( 999999m );
            SerializationTestsBase.TestSerialization( decimal.MaxValue );
            SerializationTestsBase.TestSerialization( decimal.MinValue );
            SerializationTestsBase.TestSerialization( decimal.One );
            SerializationTestsBase.TestSerialization( decimal.MinusOne );
*/
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
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (string?) null );
            this.TestSerialization( "test" );
            this.TestSerialization( string.Empty );
After:
            SerializationTestsBase.TestSerialization( (string?) null );
            SerializationTestsBase.TestSerialization( "test" );
            SerializationTestsBase.TestSerialization( string.Empty );
*/
            TestSerialization( (string?) null );
            TestSerialization( "test" );
            TestSerialization( string.Empty );
        }

        [Fact]
        public void TestDouble()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 0.0 );
            this.TestSerialization( -1.0 );
            this.TestSerialization( 1.0 );
            this.TestSerialization( double.MinValue );
            this.TestSerialization( double.MaxValue );
            this.TestSerialization( double.NaN );
            this.TestSerialization( double.NegativeInfinity );
            this.TestSerialization( double.PositiveInfinity );
After:
            SerializationTestsBase.TestSerialization( 0.0 );
            SerializationTestsBase.TestSerialization( -1.0 );
            SerializationTestsBase.TestSerialization( 1.0 );
            SerializationTestsBase.TestSerialization( double.MinValue );
            SerializationTestsBase.TestSerialization( double.MaxValue );
            SerializationTestsBase.TestSerialization( double.NaN );
            SerializationTestsBase.TestSerialization( double.NegativeInfinity );
            SerializationTestsBase.TestSerialization( double.PositiveInfinity );
*/
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
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 0.0f );
            this.TestSerialization( -1.0f );
            this.TestSerialization( 1.0f );
            this.TestSerialization( float.MinValue );
            this.TestSerialization( float.MaxValue );
            this.TestSerialization( float.NaN );
            this.TestSerialization( float.NegativeInfinity );
            this.TestSerialization( float.PositiveInfinity );
After:
            SerializationTestsBase.TestSerialization( 0.0f );
            SerializationTestsBase.TestSerialization( -1.0f );
            SerializationTestsBase.TestSerialization( 1.0f );
            SerializationTestsBase.TestSerialization( float.MinValue );
            SerializationTestsBase.TestSerialization( float.MaxValue );
            SerializationTestsBase.TestSerialization( float.NaN );
            SerializationTestsBase.TestSerialization( float.NegativeInfinity );
            SerializationTestsBase.TestSerialization( float.PositiveInfinity );
*/
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
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( Guid.Empty );
            this.TestSerialization( Guid.NewGuid() );
After:
            SerializationTestsBase.TestSerialization( Guid.Empty );
            SerializationTestsBase.TestSerialization( Guid.NewGuid() );
*/
            TestSerialization( Guid.Empty );
            TestSerialization( Guid.NewGuid() );
        }

        [Fact]
        public void TestInt16()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( short.MinValue );
            this.TestSerialization( short.MaxValue );
            this.TestSerialization( (short) 0 );
After:
            SerializationTestsBase.TestSerialization( short.MinValue );
            SerializationTestsBase.TestSerialization( short.MaxValue );
            SerializationTestsBase.TestSerialization( (short) 0 );
*/
            TestSerialization( short.MinValue );
            TestSerialization( short.MaxValue );
            TestSerialization( (short) 0 );
        }

        [Fact]
        public void TestInt64_223372036854775807()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( 223372036854775807 );
After:
            SerializationTestsBase.TestSerialization( 223372036854775807 );
*/
            TestSerialization( 223372036854775807 );
        }

        [Fact]
        public void TestInt64_m223372036854775807()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( -223372036854775807 );
After:
            SerializationTestsBase.TestSerialization( -223372036854775807 );
*/
            TestSerialization( -223372036854775807 );
        }

        [Fact]
        public void TestInt64()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( long.MinValue );
            this.TestSerialization( long.MaxValue );
            this.TestSerialization( 0L );
After:
            SerializationTestsBase.TestSerialization( long.MinValue );
            SerializationTestsBase.TestSerialization( long.MaxValue );
            SerializationTestsBase.TestSerialization( 0L );
*/
            TestSerialization( long.MinValue );
            TestSerialization( long.MaxValue );
            TestSerialization( 0L );
        }

        [Fact]
        public void TestUInt16()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( ushort.MinValue );
            this.TestSerialization( ushort.MaxValue );
            this.TestSerialization( (ushort) 0 );
After:
            SerializationTestsBase.TestSerialization( ushort.MinValue );
            SerializationTestsBase.TestSerialization( ushort.MaxValue );
            SerializationTestsBase.TestSerialization( (ushort) 0 );
*/
            TestSerialization( ushort.MinValue );
            TestSerialization( ushort.MaxValue );
            TestSerialization( (ushort) 0 );
        }

        [Fact]
        public void TestUInt64()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( ulong.MinValue );
            this.TestSerialization( ulong.MaxValue );
            this.TestSerialization( 0UL );
After:
            SerializationTestsBase.TestSerialization( ulong.MinValue );
            SerializationTestsBase.TestSerialization( ulong.MaxValue );
            SerializationTestsBase.TestSerialization( 0UL );
*/
            TestSerialization( ulong.MinValue );
            TestSerialization( ulong.MaxValue );
            TestSerialization( 0UL );
        }

        [Fact]
        public void TestUInt32()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( uint.MinValue );
            this.TestSerialization( uint.MaxValue );
            this.TestSerialization( 0U );
After:
            SerializationTestsBase.TestSerialization( uint.MinValue );
            SerializationTestsBase.TestSerialization( uint.MaxValue );
            SerializationTestsBase.TestSerialization( 0U );
*/
            TestSerialization( uint.MinValue );
            TestSerialization( uint.MaxValue );
            TestSerialization( 0U );
        }

        [Fact]
        public void TestNullableInt()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (int?) 0 );
            this.TestSerialization( (int?) int.MaxValue );
            this.TestSerialization( (int?) int.MinValue );
            this.TestSerialization( (int?) null );
After:
            SerializationTestsBase.TestSerialization( (int?) 0 );
            SerializationTestsBase.TestSerialization( (int?) int.MaxValue );
            SerializationTestsBase.TestSerialization( (int?) int.MinValue );
            SerializationTestsBase.TestSerialization( (int?) null );
*/
            TestSerialization( (int?) 0 );
            TestSerialization( (int?) int.MaxValue );
            TestSerialization( (int?) int.MinValue );
            TestSerialization( (int?) null );
        }

        [Fact]
        public void TestNullableEnum()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (TypeCode?) TypeCode.Boolean );
            this.TestSerialization( (TypeCode?) null );
After:
            SerializationTestsBase.TestSerialization( (TypeCode?) TypeCode.Boolean );
            SerializationTestsBase.TestSerialization( (TypeCode?) null );
*/
            TestSerialization( (TypeCode?) TypeCode.Boolean );
            TestSerialization( (TypeCode?) null );
        }

        [Fact]
        public void TestArray_Ints()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( new[] { 1, int.MinValue, 9, int.MaxValue } );
After:
            SerializationTestsBase.TestSerialization( new[] { 1, int.MinValue, 9, int.MaxValue } );
*/
            TestSerialization( new[] { 1, int.MinValue, 9, int.MaxValue } );
        }

        [Fact]
        public void TestArray_Objects()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( new[] { new SimpleType(), new SimpleType(), new SimpleType(), new SimpleType() } );
After:
            SerializationTestsBase.TestSerialization( new[] { new SimpleType(), new SimpleType(), new SimpleType(), new SimpleType() } );
*/
            TestSerialization( new[] { new SimpleType(), new SimpleType(), new SimpleType(), new SimpleType() } );
        }

        [Fact]
        public void TestArray_Structs()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( new[] { DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue } );
After:
            SerializationTestsBase.TestSerialization( new[] { DateTime.Now, DateTime.Today, DateTime.MinValue, DateTime.MaxValue } );
*/
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

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( array );
After:
            SerializationTestsBase.TestSerialization( array );
*/
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
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( new object[] { DateTime.Now, 11, false, "test", 0.0 } );
After:
            SerializationTestsBase.TestSerialization( new object[] { DateTime.Now, 11, false, "test", 0.0 } );
*/
            TestSerialization( new object[] { DateTime.Now, 11, false, "test", 0.0 } );
        }

        [Fact]
        public void TestArray_WithNulls()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( new object[2] );
After:
            SerializationTestsBase.TestSerialization( new object[2] );
*/
            TestSerialization( new object[2] );
        }

        [Fact]
        public void TestBoxedIntrinsics()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (object) 5 );
            this.TestSerialization( (object) -13.0 );
            this.TestSerialization( (object) "test" );
            this.TestSerialization( (object) DateTime.Now );
After:
            SerializationTestsBase.TestSerialization( (object) 5 );
            SerializationTestsBase.TestSerialization( (object) -13.0 );
            SerializationTestsBase.TestSerialization( (object) "test" );
            SerializationTestsBase.TestSerialization( (object) DateTime.Now );
*/
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