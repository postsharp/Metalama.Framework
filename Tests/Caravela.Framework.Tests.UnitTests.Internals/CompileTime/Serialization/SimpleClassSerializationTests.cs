// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime.Serialization;
using Caravela.Framework.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public class SimpleClassSerializationTests : SerializationTestsBase
    {
        [Fact]
        public void TestClassWithString_SimpleWord()
        {
            this.TestSimpleExplicitelySerializedClass( "SimpleText" );
        }

        [Fact]
        public void TestClassWithString_SimpleDottedWords()
        {
            this.TestSimpleExplicitelySerializedClass( "Simple.Dotted.Words" );
        }

        [Fact]
        public void TestClassWithString_DottedWordsWithReservedNames()
        {
            this.TestSimpleExplicitelySerializedClass( "Simple.Dotted.Words, mscorlib" );
        }

        [Fact]
        public void TestClassWithBoxedInt()
        {
            this.TestSimpleExplicitelySerializedClass<object>( 1000 );
        }

        [Fact]
        public void TestClassWithBoxedBool()
        {
            this.TestSimpleExplicitelySerializedClass<object>( false );
        }

        [Fact]
        public void TestClassWithBoxedStruct()
        {
            this.TestSimpleExplicitelySerializedClass<object>( DateTime.Now );
        }

        [Fact]
        public void TestClassWithInt32_1000()
        {
            this.TestSimpleExplicitelySerializedClass( 1000 );
        }

        [Fact]
        public void TestClassWithInt32_m1000()
        {
            this.TestSimpleExplicitelySerializedClass( -1000 );
        }

        [Fact]
        public void TestClassWithInt32_1000000000()
        {
            this.TestSimpleExplicitelySerializedClass( 1000000000 );
        }

        [Fact]
        public void TestClassWithInt32_m1000000000()
        {
            this.TestSimpleExplicitelySerializedClass( -1000000000 );
        }

        [Fact]
        public void TestClassWithInt64_223372036854775807()
        {
            this.TestSimpleExplicitelySerializedClass( 223372036854775807 );
        }

        [Fact]
        public void TestClassWithInt64_m223372036854775807()
        {
            this.TestSimpleExplicitelySerializedClass( -223372036854775807 );
        }

        [Fact]
        public void TestClassWithDouble_1000()
        {
            this.TestSimpleExplicitelySerializedClass( 1000d );
        }

        [Fact]
        public void TestClassWithDouble_m1000()
        {
            this.TestSimpleExplicitelySerializedClass( -1000d );
        }

        [Fact]
        public void TestClassWithDouble_Max()
        {
            this.TestSimpleExplicitelySerializedClass( Double.MaxValue );
        }

        [Fact]
        public void TestClassWithDouble_Min()
        {
            this.TestSimpleExplicitelySerializedClass( Double.MinValue );
        }

        [Fact]
        public void TestClassWithStruct_DateTime()
        {
            this.TestSimpleExplicitelySerializedClass( DateTime.Now );
        }

        [Fact]
        public void TestClasWithObjectMember_Nulled()
        {
            this.TestSimpleExplicitelySerializedClass( (object)null );
        }

        [Fact]
        public void TestClasWithStringMember_Nulled()
        {
            this.TestSimpleExplicitelySerializedClass( (string)null );
        }

        [Fact]
        public void TestClasWithClassMember_Nulled()
        {
            this.TestSimpleExplicitelySerializedClass( (SimpleExplicitelySerializedClass<int>)null );
        }

        [Fact]
        public void TestClasWithClassMember_NotNulled()
        {
            this.TestSimpleExplicitelySerializedClass( new SimpleExplicitelySerializedClass<string>( "testing text" ) );
        }

        [Fact]
        public void TestClasWithEnum_SimpleValue()
        {
            this.TestSimpleExplicitelySerializedClass( TestEnum.Value1 );
        }

        [Fact]
        public void TestClasWithEnum_NonZeroValue()
        {
            this.TestSimpleExplicitelySerializedClass( TestEnum.Value2 );
        }

        [Fact]
        public void TestClasWithLongEnum_NonZeroValue()
        {
            this.TestSimpleExplicitelySerializedClass( TestEnumWithLong.Value2 );
        }

        [Fact]
        public void TestClasWithByteEnum_NonZeroValue()
        {
            this.TestSimpleExplicitelySerializedClass( TestEnumWithByte.Value2 );
        }

        [Fact]
        public void TestClasWithFlagsEnum_NonZeroValue()
        {
            this.TestSimpleExplicitelySerializedClass( TestEnumWithFlags.Value2 );
        }

        [Fact]
        public void TestClasWithFlagsEnum_MultiValue()
        {
            this.TestSimpleExplicitelySerializedClass( TestEnumWithFlags.Value1 | TestEnumWithFlags.Value2 );
        }

        [Fact]
        public void TestClassWithProperty_Int1()
        {
            this.TestExplicitlySerializedClass( (object)null, 1 );
        }

        [Fact]
        public void TestClassWithArray_Int()
        {
            this.TestSimpleExplicitelySerializedClass( new[] { 1, 2, 3, 4 } );
        }

        [Fact]
        public void TestClassWithLargeArray_Int()
        {
            int[] array = new int[100000];

            for ( int i = 0; i < array.Length; i++ )
            {
                array[i] = i;
            }

            this.TestSimpleExplicitelySerializedClass( array );
        }

        [Fact]
        public void TestClassWithRank2Array()
        {
            this.TestSimpleExplicitelySerializedClass( new[,] { { 1, 2 }, { 3, 4 } } );
        }

        [Fact]
        public void TestClassWithRank3Array()
        {
            this.TestSimpleExplicitelySerializedClass( new[,,] { { { 1 }, { 2 } }, { { 3 }, { 4 } } } );
        }

        [Fact]
        public void TestClassWithMultidimensionalRank1Array()
        {
            //We're creating instance of type int[*], but actually get int[] -> and that's what were testing
            Array array = (Array)Activator.CreateInstance( typeof(int).MakeArrayType( 1 ), 2 );
            array.SetValue( 1, 0 );
            array.SetValue( 2, 1 );
            this.TestSimpleExplicitelySerializedClass( (int[])array );
        }

        [Fact]
        public void TestClassWithValueAndProperty_Int1AndInt1()
        {
            this.TestExplicitlySerializedClass( 1, 1 );
        }

        [Fact]
        public void TestClassWithValueAndProperty_StringAndString()
        {
            this.TestExplicitlySerializedClass( "a field", "a property" );
        }

        [Fact]
        public void TestClassWithValueAndProperty_BothNulled()
        {
            this.TestExplicitlySerializedClass( (object)null, (object)null );
        }

        [Fact]
        public void TestClassWithNullableInt_NotNull()
        {
            this.TestSerialization( new ExplicitlySerializedClass<int, int>( 5 ) { Nullable = 3 } );
        }

        [Fact]
        public void TestHeterogeneousArrayOfReferenceTypeObjects()
        {
            SimpleExplicitelySerializedClass<int>[] array = new SimpleExplicitelySerializedClass<int>[3];
            array[0] = new SimpleExplicitelySerializedClass<int>( 5 );
            array[1] = new ExplicitlySerializedClass<int, string>( 2 );
            this.TestSerialization( array );
        }

        [Fact]
        public void GenericTypes_InArray()
        {
            SimpleExplicitelySerializedClass<DateTime> serializedClass = new SimpleExplicitelySerializedClass<DateTime>( DateTime.Today.AddDays( 10 ) );
            SimpleExplicitelySerializedClass<DateTime> anotherSerializedClass = new SimpleExplicitelySerializedClass<DateTime>(
                DateTime.Today.AddMonths( -10 ) );
            SimpleExplicitelySerializedClass<DateTime>[] array = new[] { serializedClass, anotherSerializedClass };

            MetaFormatter formatter = new MetaFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize( array, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            SimpleExplicitelySerializedClass<DateTime>[] deserializedObject =
                (SimpleExplicitelySerializedClass<DateTime>[])formatter.Deserialize( memoryStream );

            Assert.NotNull( deserializedObject );
            Assert.Equal( 2, deserializedObject.Length );
            Assert.Equal( serializedClass, deserializedObject[0] );
            Assert.Equal( anotherSerializedClass, deserializedObject[1] );
        }

        private void TestSimpleExplicitelySerializedClass<T>( T value )
        {
            SimpleExplicitelySerializedClass<T> initialObject = new SimpleExplicitelySerializedClass<T>( value );
            MetaFormatter formatter = new MetaFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize( initialObject, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            SimpleExplicitelySerializedClass<T> deserializedObject = (SimpleExplicitelySerializedClass<T>)formatter.Deserialize( memoryStream );

            if ( typeof(T).IsArray )
            {
                Assert.Equal( (ICollection)initialObject.Value, (ICollection)deserializedObject.Value );
            }
            else
            {
                Assert.Equal( initialObject.Value, deserializedObject.Value );
            }
        }

        private void TestExplicitlySerializedClass<TForCtor, TForField>( TForCtor value, TForField property )
        {
            ExplicitlySerializedClass<TForCtor, TForField> initialObject = new ExplicitlySerializedClass<TForCtor, TForField>( value );
            initialObject.Field = property;
            MetaFormatter formatter = new MetaFormatter();
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize( initialObject, memoryStream );
            memoryStream.Seek( 0, SeekOrigin.Begin );
            ExplicitlySerializedClass<TForCtor, TForField> deserializedObject =
                (ExplicitlySerializedClass<TForCtor, TForField>)formatter.Deserialize( memoryStream );

            Assert.Equal( initialObject.Value, deserializedObject.Value );
            Assert.Equal( initialObject.Field, deserializedObject.Field );
        }

        [MetaSerializer( typeof(SimpleExplicitelySerializedClass<>.Serializer) )]
        public class SimpleExplicitelySerializedClass<T> : IEquatable<SimpleExplicitelySerializedClass<T>>
        {
            public T Value;

            public SimpleExplicitelySerializedClass( T value )
            {
                this.Value = value;
            }

            public bool Equals( SimpleExplicitelySerializedClass<T> other )
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

            public override bool Equals( object obj )
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
                return this.Equals( (SimpleExplicitelySerializedClass<T>)obj );
            }

            public override int GetHashCode()
            {
                return EqualityComparer<T>.Default.GetHashCode( this.Value );
            }

            public class Serializer : ReferenceTypeSerializer<SimpleExplicitelySerializedClass<T>>
            {
                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new SimpleExplicitelySerializedClass<T>( constructorArguments.GetValue<T>( "_" ) );
                }

                public override void SerializeObject( SimpleExplicitelySerializedClass<T> obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    constructorArguments.SetValue( "_", obj.Value );
                }

                public override void DeserializeFields( SimpleExplicitelySerializedClass<T> obj, IArgumentsReader initializationArguments )
                {
                }
            }
        }

        [MetaSerializer( typeof(ExplicitlySerializedClass<,>.Serializer) )]
        public class ExplicitlySerializedClass<TForCtor, TForField> : SimpleExplicitelySerializedClass<TForCtor>,
                                                                       IEquatable<ExplicitlySerializedClass<TForCtor, TForField>>
        {
            public ExplicitlySerializedClass( TForCtor value )
                : base( value )
            {
            }

            public TForField Field { get; set; }

            public int? Nullable { get; set; }

            public bool Equals( ExplicitlySerializedClass<TForCtor, TForField> other )
            {
                if ( ReferenceEquals( null, other ) )
                {
                    return false;
                }
                if ( ReferenceEquals( this, other ) )
                {
                    return true;
                }
                return base.Equals( other ) && EqualityComparer<TForField>.Default.Equals( this.Field, other.Field ) && this.Nullable == other.Nullable;
            }

            public override bool Equals( object obj )
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
                return this.Equals( (ExplicitlySerializedClass<TForCtor, TForField>)obj );
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = base.GetHashCode();
                    hashCode = (hashCode * 397) ^ EqualityComparer<TForField>.Default.GetHashCode( this.Field );
                    hashCode = (hashCode * 397) ^ this.Nullable.GetHashCode();
                    return hashCode;
                }
            }

            public new class Serializer : ReferenceTypeSerializer<ExplicitlySerializedClass<TForCtor, TForField>>
            {
                private const string valueKey = "_ov";

                private const string fieldKey = "_fv";

                public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
                {
                    return new ExplicitlySerializedClass<TForCtor, TForField>( constructorArguments.GetValue<TForCtor>( valueKey ) );
                }

                public override void SerializeObject( ExplicitlySerializedClass<TForCtor, TForField> obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
                {
                    constructorArguments.SetValue( valueKey, obj.Value );
                    initializationArguments.SetValue( fieldKey, obj.Field );
                    initializationArguments.SetValue( "n", obj.Nullable );
                }

                public override void DeserializeFields( ExplicitlySerializedClass<TForCtor, TForField> obj, IArgumentsReader initializationArguments )
                {
                    obj.Field = initializationArguments.GetValue<TForField>( fieldKey );
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

            Value2,
        }

        public enum TestEnumWithByte : byte
        {
            Value1,

            Value2,
        }

        [Flags]
        public enum TestEnumWithFlags
        {
            Value1 = 0x01,

            Value2 = 0x02,
        }
    }
}