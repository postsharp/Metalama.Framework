// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime.Serialization;
using Metalama.Framework.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xunit;

#pragma warning disable SA1500 // Braces for multi-line statements should not share line

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public class SimpleClassSerializationTests : SerializationTestsBase
    {
        [Fact]
        public void TestClassWithString_SimpleWord()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( "SimpleText" );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( "SimpleText" );
*/
            TestSimpleExplicitlySerializedClass( "SimpleText" );
        }

        [Fact]
        public void TestClassWithString_SimpleDottedWords()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( "Simple.Dotted.Words" );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( "Simple.Dotted.Words" );
*/
            TestSimpleExplicitlySerializedClass( "Simple.Dotted.Words" );
        }

        [Fact]
        public void TestClassWithString_DottedWordsWithReservedNames()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( "Simple.Dotted.Words, mscorlib" );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( "Simple.Dotted.Words, mscorlib" );
*/
            TestSimpleExplicitlySerializedClass( "Simple.Dotted.Words, mscorlib" );
        }

        [Fact]
        public void TestClassWithBoxedInt()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass<object>( 1000 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass<object>( 1000 );
*/
            TestSimpleExplicitlySerializedClass<object>( 1000 );
        }

        [Fact]
        public void TestClassWithBoxedBool()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass<object>( false );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass<object>( false );
*/
            TestSimpleExplicitlySerializedClass<object>( false );
        }

        [Fact]
        public void TestClassWithBoxedStruct()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass<object>( DateTime.Now );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass<object>( DateTime.Now );
*/
            TestSimpleExplicitlySerializedClass<object>( DateTime.Now );
        }

        [Fact]
        public void TestClassWithInt32_1000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( 1000 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( 1000 );
*/
            TestSimpleExplicitlySerializedClass( 1000 );
        }

        [Fact]
        public void TestClassWithInt32_m1000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( -1000 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( -1000 );
*/
            TestSimpleExplicitlySerializedClass( -1000 );
        }

        [Fact]
        public void TestClassWithInt32_1000000000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( 1000000000 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( 1000000000 );
*/
            TestSimpleExplicitlySerializedClass( 1000000000 );
        }

        [Fact]
        public void TestClassWithInt32_m1000000000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( -1000000000 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( -1000000000 );
*/
            TestSimpleExplicitlySerializedClass( -1000000000 );
        }

        [Fact]
        public void TestClassWithInt64_223372036854775807()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( 223372036854775807 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( 223372036854775807 );
*/
            TestSimpleExplicitlySerializedClass( 223372036854775807 );
        }

        [Fact]
        public void TestClassWithInt64_m223372036854775807()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( -223372036854775807 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( -223372036854775807 );
*/
            TestSimpleExplicitlySerializedClass( -223372036854775807 );
        }

        [Fact]
        public void TestClassWithDouble_1000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( 1000d );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( 1000d );
*/
            TestSimpleExplicitlySerializedClass( 1000d );
        }

        [Fact]
        public void TestClassWithDouble_m1000()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( -1000d );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( -1000d );
*/
            TestSimpleExplicitlySerializedClass( -1000d );
        }

        [Fact]
        public void TestClassWithDouble_Max()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( double.MaxValue );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( double.MaxValue );
*/
            TestSimpleExplicitlySerializedClass( double.MaxValue );
        }

        [Fact]
        public void TestClassWithDouble_Min()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( double.MinValue );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( double.MinValue );
*/
            TestSimpleExplicitlySerializedClass( double.MinValue );
        }

        [Fact]
        public void TestClassWithStruct_DateTime()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( DateTime.Now );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( DateTime.Now );
*/
            TestSimpleExplicitlySerializedClass( DateTime.Now );
        }

        [Fact]
        public void TestClasWithObjectMember_Null()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( (object?) null );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( (object?) null );
*/
            TestSimpleExplicitlySerializedClass( (object?) null );
        }

        [Fact]
        public void TestClasWithStringMember_Null()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( (string?) null );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( (string?) null );
*/
            TestSimpleExplicitlySerializedClass( (string?) null );
        }

        [Fact]
        public void TestClasWithClassMember_Null()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( (SimpleExplicitlySerializedClass<int>?) null );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( (SimpleExplicitlySerializedClass<int>?) null );
*/
            TestSimpleExplicitlySerializedClass( (SimpleExplicitlySerializedClass<int>?) null );
        }

        [Fact]
        public void TestClasWithClassMember_NotNull()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( new SimpleExplicitlySerializedClass<string>( "testing text" ) );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( new SimpleExplicitlySerializedClass<string>( "testing text" ) );
*/
            TestSimpleExplicitlySerializedClass( new SimpleExplicitlySerializedClass<string>( "testing text" ) );
        }

        [Fact]
        public void TestClasWithEnum_SimpleValue()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( TestEnum.Value1 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( TestEnum.Value1 );
*/
            TestSimpleExplicitlySerializedClass( TestEnum.Value1 );
        }

        [Fact]
        public void TestClasWithEnum_NonZeroValue()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( TestEnum.Value2 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( TestEnum.Value2 );
*/
            TestSimpleExplicitlySerializedClass( TestEnum.Value2 );
        }

        [Fact]
        public void TestClasWithLongEnum_NonZeroValue()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( TestEnumWithLong.Value2 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( TestEnumWithLong.Value2 );
*/
            TestSimpleExplicitlySerializedClass( TestEnumWithLong.Value2 );
        }

        [Fact]
        public void TestClasWithByteEnum_NonZeroValue()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( TestEnumWithByte.Value2 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( TestEnumWithByte.Value2 );
*/
            TestSimpleExplicitlySerializedClass( TestEnumWithByte.Value2 );
        }

        [Fact]
        public void TestClasWithFlagsEnum_NonZeroValue()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( TestEnumWithFlags.Value2 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( TestEnumWithFlags.Value2 );
*/
            TestSimpleExplicitlySerializedClass( TestEnumWithFlags.Value2 );
        }

        [Fact]
        public void TestClasWithFlagsEnum_MultiValue()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2 );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2 );
*/
            TestSimpleExplicitlySerializedClass( TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2 );
        }

        [Fact]
        public void TestClassWithProperty_Int1()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestExplicitlySerializedClass( (object) null, 1 );
After:
            SimpleClassSerializationTests.TestExplicitlySerializedClass( (object) null, 1 );
*/
            TestExplicitlySerializedClass( (object?) null, 1 );
        }

        [Fact]
        public void TestClassWithArray_Int()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( new[] { 1, 2, 3, 4 } );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( new[] { 1, 2, 3, 4 } );
*/
            TestSimpleExplicitlySerializedClass( new[] { 1, 2, 3, 4 } );
        }

        [Fact]
        public void TestClassWithLargeArray_Int()
        {
            var array = new int[100000];

            for ( var i = 0; i < array.Length; i++ )
            {
                array[i] = i;
            }

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( array );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( array );
*/
            TestSimpleExplicitlySerializedClass( array );
        }

        [Fact]
        public void TestClassWithRank2Array()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( new[,] { { 1, 2 }, { 3, 4 } } );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( new[,] { { 1, 2 }, { 3, 4 } } );
*/
            TestSimpleExplicitlySerializedClass( new[,] { { 1, 2 }, { 3, 4 } } );
        }

        [Fact]
        public void TestClassWithRank3Array()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( new[, ,] { { { 1 }, { 2 } }, { { 3 }, { 4 } } } );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( new[, ,] { { { 1 }, { 2 } }, { { 3 }, { 4 } } } );
*/
            TestSimpleExplicitlySerializedClass( new[,,] { { { 1 }, { 2 } }, { { 3 }, { 4 } } } );
        }

        [Fact]
        public void TestClassWithMultidimensionalRank1Array()
        {
            // We're creating instance of type int[*], but actually get int[] -> and that's what were testing
            var array = (Array?) Activator.CreateInstance( typeof(int).MakeArrayType( 1 ), 2 );
            array!.SetValue( 1, 0 );
            array.SetValue( 2, 1 );

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSimpleExplicitlySerializedClass( (int[]) array );
After:
            SimpleClassSerializationTests.TestSimpleExplicitlySerializedClass( (int[]) array );
*/
            TestSimpleExplicitlySerializedClass( (int[]) array );
        }

        [Fact]
        public void TestClassWithValueAndProperty_Int1AndInt1()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestExplicitlySerializedClass( 1, 1 );
After:
            SimpleClassSerializationTests.TestExplicitlySerializedClass( 1, 1 );
*/
            TestExplicitlySerializedClass( 1, 1 );
        }

        [Fact]
        public void TestClassWithValueAndProperty_StringAndString()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestExplicitlySerializedClass( "a field", "a property" );
After:
            SimpleClassSerializationTests.TestExplicitlySerializedClass( "a field", "a property" );
*/
            TestExplicitlySerializedClass( "a field", "a property" );
        }

        [Fact]
        public void TestClassWithValueAndProperty_BothNull()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestExplicitlySerializedClass( (object?) null, (object?) null );
After:
            SimpleClassSerializationTests.TestExplicitlySerializedClass( (object?) null, (object?) null );
*/
            TestExplicitlySerializedClass( (object?) null, (object?) null );
        }

        [Fact]
        public void TestClassWithNullableInt_NotNull()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( new ExplicitlySerializedClass<int, int>( 5 ) { Nullable = 3 } );
After:
            SerializationTestsBase.TestSerialization( new ExplicitlySerializedClass<int, int>( 5 ) { Nullable = 3 } );
*/
            TestSerialization( new ExplicitlySerializedClass<int, int>( 5 ) { Nullable = 3 } );
        }

        [Fact]
        public void TestHeterogeneousArrayOfReferenceTypeObjects()
        {
            var array = new SimpleExplicitlySerializedClass<int>[3];
            array[0] = new SimpleExplicitlySerializedClass<int>( 5 );
            array[1] = new ExplicitlySerializedClass<int, string>( 2 );

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( array );
After:
            SerializationTestsBase.TestSerialization( array );
*/
            TestSerialization( array );
        }

        [Fact]
        public void GenericTypes_InArray()
        {
            var serializedClass = new SimpleExplicitlySerializedClass<DateTime>( DateTime.Today.AddDays( 10 ) );
            var anotherSerializedClass = new SimpleExplicitlySerializedClass<DateTime>( DateTime.Today.AddMonths( -10 ) );
            var array = new[] { serializedClass, anotherSerializedClass };

            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( array, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );

            var deserializedObject =
                (SimpleExplicitlySerializedClass<DateTime>[]?) formatter.Deserialize( memoryStream );

            Assert.NotNull( deserializedObject );
            Assert.Equal( 2, deserializedObject!.Length );
            Assert.Equal( serializedClass, deserializedObject[0] );
            Assert.Equal( anotherSerializedClass, deserializedObject[1] );
        }

        private static void TestSimpleExplicitlySerializedClass<T>( T value )
        {
            var initialObject = new SimpleExplicitlySerializedClass<T>( value );
            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( initialObject, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            var deserializedObject = (SimpleExplicitlySerializedClass<T>?) formatter.Deserialize( memoryStream );

            if ( typeof(T).IsArray )
            {
                Assert.Equal( (ICollection?) initialObject.Value, (ICollection?) deserializedObject!.Value );
            }
            else
            {
                Assert.Equal( initialObject.Value, deserializedObject!.Value );
            }
        }

        private static void TestExplicitlySerializedClass<TForCtor, TForField>( TForCtor value, TForField property )
        {
            var initialObject = new ExplicitlySerializedClass<TForCtor, TForField>( value ) { Field = property };
            var formatter = new MetaFormatter();
            var memoryStream = new MemoryStream();
            formatter.Serialize( initialObject, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );

            var deserializedObject =
                (ExplicitlySerializedClass<TForCtor, TForField>?) formatter.Deserialize( memoryStream );

            Assert.Equal( initialObject.Value, deserializedObject!.Value );
            Assert.Equal( initialObject.Field, deserializedObject.Field );
        }

        [MetaSerializer( typeof(SimpleExplicitlySerializedClass<>.Serializer) )]
        public class SimpleExplicitlySerializedClass<T> : IEquatable<SimpleExplicitlySerializedClass<T>>
        {
#pragma warning disable SA1401 // Fields should be private
            public T Value;
#pragma warning restore SA1401 // Fields should be private

            public SimpleExplicitlySerializedClass( T value )
            {
                this.Value = value;
            }

            public bool Equals( SimpleExplicitlySerializedClass<T>? other )
            {
                if ( ReferenceEquals( null, other ) )
                {
                    return false;
                }

                if ( ReferenceEquals( this, other ) )
                {
                    return true;
                }

                return EqualityComparer<T>.Default.Equals( this.Value, other.Value );
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

                return this.Equals( (SimpleExplicitlySerializedClass<T>) obj );
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return EqualityComparer<T>.Default.GetHashCode( this.Value! );
            }

            public class Serializer : ReferenceTypeSerializer<SimpleExplicitlySerializedClass<T>>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new SimpleExplicitlySerializedClass<T>( constructorArguments.GetValue<T>( "_" )! );
                }

                public override void SerializeObject(
                    SimpleExplicitlySerializedClass<T> obj,
                    IArgumentsWriter constructorArguments,
                    IArgumentsWriter initializationArguments )
                {
                    constructorArguments.SetValue( "_", obj.Value );
                }

                public override void DeserializeFields( SimpleExplicitlySerializedClass<T> obj, IArgumentsReader initializationArguments ) { }
            }
        }

        [MetaSerializer( typeof(ExplicitlySerializedClass<,>.Serializer) )]
        public class ExplicitlySerializedClass<TForCtor, TForField> : SimpleExplicitlySerializedClass<TForCtor>,
                                                                      IEquatable<ExplicitlySerializedClass<TForCtor, TForField>>
        {
            public TForField? Field { get; set; }

            public int? Nullable { get; set; }

            public ExplicitlySerializedClass( TForCtor value )
                : base( value ) { }

            public bool Equals( ExplicitlySerializedClass<TForCtor, TForField>? other )
            {
                if ( ReferenceEquals( null, other ) )
                {
                    return false;
                }

                if ( ReferenceEquals( this, other ) )
                {
                    return true;
                }

                return base.Equals( other ) && EqualityComparer<TForField>.Default.Equals( this.Field!, other.Field! ) && this.Nullable == other.Nullable;
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

                return this.Equals( (ExplicitlySerializedClass<TForCtor, TForField>) obj );
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    // ReSharper disable NonReadonlyMemberInGetHashCode

                    var hashCode = base.GetHashCode();
                    hashCode = (hashCode * 397) ^ EqualityComparer<TForField>.Default.GetHashCode( this.Field! );
                    hashCode = (hashCode * 397) ^ this.Nullable.GetHashCode();

                    // ReSharper restore NonReadonlyMemberInGetHashCode
                    
                    return hashCode;
                }
            }

            public new class Serializer : ReferenceTypeSerializer<ExplicitlySerializedClass<TForCtor, TForField>>
            {
                private const string _valueKey = "_ov";
                private const string _fieldKey = "_fv";

                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new ExplicitlySerializedClass<TForCtor, TForField>( constructorArguments.GetValue<TForCtor>( _valueKey )! );
                }

                public override void SerializeObject(
                    ExplicitlySerializedClass<TForCtor, TForField> obj,
                    IArgumentsWriter constructorArguments,
                    IArgumentsWriter initializationArguments )
                {
                    constructorArguments.SetValue( _valueKey, obj.Value );
                    initializationArguments.SetValue( _fieldKey, obj.Field );
                    initializationArguments.SetValue( "n", obj.Nullable );
                }

                public override void DeserializeFields( ExplicitlySerializedClass<TForCtor, TForField> obj, IArgumentsReader initializationArguments )
                {
                    obj.Field = initializationArguments.GetValue<TForField>( _fieldKey );
                    obj.Nullable = initializationArguments.GetValue<int?>( "n" );
                }
            }
        }

        public enum TestEnum
        {
            Value1,

            Value2,

            Value3
        }

        public enum TestEnumWithLong : long
        {
            Value1,

            Value2
        }

        public enum TestEnumWithByte : byte
        {
            Value1,

            Value2
        }

        [Flags]
        public enum TestEnumWithFlags
        {
            Value1 = 0x01,

            Value2 = 0x02
        }
    }
}