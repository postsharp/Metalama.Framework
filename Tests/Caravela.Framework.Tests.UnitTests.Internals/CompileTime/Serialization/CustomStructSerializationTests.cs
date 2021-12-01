using Caravela.Framework.Impl.CompileTime.Serialization;
using Caravela.Framework.Serialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime.Serialization
{
    
    public class CustomStructSerializationTests : SerializationTestsBase
    {
        
        [Fact]
        public void SerializeStruct_BasicTest()
        {
            SimpleStruct s = new SimpleStruct(1, DateTime.Now);
            s.StringValue = "Test";
            s.NullableEnumField = TypeCode.Char;
            TestSerialization( s );
        }

        [Fact]
        public void SerializeStruct_BoxedStruct()
        {
            SimpleStruct s = new SimpleStruct(1, DateTime.Now);
            s.StringValue = "Test";
            TestSerialization((object)s);
        }

        [Fact]
        public void SerializeStruct_GenericStruct()
        {
            GenericStruct<string> s = new GenericStruct<string>();
            s.Value = "1";
            TestSerialization( s );
        }

        [Fact]
        public void SerializeStruct_NestedStructs()
        {
            GenericStruct<SimpleStruct> s = new GenericStruct<SimpleStruct>();
            s.Value = new SimpleStruct(5, DateTime.MinValue);
            TestSerialization(s);
        }

        [Fact]
        public void SerializeStruct_WithObjectReferences()
        {
            SimpleClass cls = new SimpleClass( 11 );
            SimpleStruct str = new SimpleStruct( 1, DateTime.Now ) {SimpleClass = cls, SimpleClass2 = cls};
            SimpleStruct str2 = TestSerialization( str );
            Assert.Same( str2.SimpleClass, str2.SimpleClass2 );
        }

        #region GenericStruct

        [MetaSerializer(typeof(GenericStructSerializer<>))]
        public struct GenericStruct<T> : IEquatable<GenericStruct<T>>
        {
            public T Value { get; set; }

            public string StringValue { get; set; }

            public bool Equals( GenericStruct<T> other )
            {
                return EqualityComparer<T>.Default.Equals( Value, other.Value ) && string.Equals( StringValue, other.StringValue );
            }

            public override bool Equals( object obj )
            {
                if ( ReferenceEquals( null, obj ) ) return false;
                return obj is GenericStruct<T> && Equals( (GenericStruct<T>) obj );
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (EqualityComparer<T>.Default.GetHashCode( Value )*397) ^ (StringValue != null ? StringValue.GetHashCode() : 0);
                }
            }
        }

        public class GenericStructSerializer<T> : ValueTypeMetaSerializer<GenericStruct<T>>
        {
            public override void SerializeObject( GenericStruct<T> value, IArgumentsWriter writer )
            {
                writer.SetValue( "T", value.Value );
                writer.SetValue( "s", value.StringValue );
            }

            public override GenericStruct<T> DeserializeObject( IArgumentsReader reader )
            {
                GenericStruct<T> s = new GenericStruct<T>();
                s.StringValue = reader.GetValue<string>( "s" );
                s.Value = reader.GetValue<T>( "T" );
                return s;
            }
        }

        #endregion


        #region SimpleStruct

        [MetaSerializer(typeof(SimpleStructSerializer))]
        public struct SimpleStruct : IEquatable<SimpleStruct>
        {
            public int IntValue { get; set; }

            public string StringValue { get; set; }

            public DateTime TimeValue { get; set; }

            public TypeCode? NullableEnumField { get; set; }

            public SimpleClass SimpleClass { get; set; }

            public SimpleClass SimpleClass2 { get; set; }

            public SimpleStruct( int intValue, DateTime timeValue ) : this()
            {
                IntValue = intValue;
                TimeValue = timeValue;
            }

            public bool Equals( SimpleStruct other )
            {
                return IntValue == other.IntValue && string.Equals( StringValue, other.StringValue ) && TimeValue.Equals( other.TimeValue ) && Equals( SimpleClass, other.SimpleClass ) && 
                    Equals( SimpleClass2, other.SimpleClass2 ) && Equals( NullableEnumField, other.NullableEnumField);
            }

            public override bool Equals( object obj )
            {
                if ( ReferenceEquals( null, obj ) ) return false;
                return obj is SimpleStruct && Equals( (SimpleStruct) obj );
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = IntValue;
                    hashCode = (hashCode*397) ^ (StringValue != null ? StringValue.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ TimeValue.GetHashCode();
                    hashCode = (hashCode*397) ^ (SimpleClass != null ? SimpleClass.GetHashCode() : 0);
                    hashCode = (hashCode*397) ^ (SimpleClass2 != null ? SimpleClass2.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (NullableEnumField != null ? NullableEnumField.GetHashCode() : 0);
                    return hashCode;
                }
            }
        }

        public class SimpleStructSerializer : ValueTypeMetaSerializer<SimpleStruct>
        {
            public override void SerializeObject( SimpleStruct value, IArgumentsWriter writer )
            {
                writer.SetValue("int", value.IntValue);
                writer.SetValue("time", value.TimeValue);
                writer.SetValue( "string", value.StringValue);
                writer.SetValue( "cls", value.SimpleClass );
                writer.SetValue("cls2", value.SimpleClass2);
                writer.SetValue("ne", value.NullableEnumField);
            }

            public override SimpleStruct DeserializeObject( IArgumentsReader reader )
            {
                SimpleStruct str = new SimpleStruct( reader.GetValue<int>( "int" ), reader.GetValue<DateTime>( "time" ) );
                str.StringValue = reader.GetValue<string>( "string" );
                str.SimpleClass = reader.GetValue<SimpleClass>( "cls" );
                str.SimpleClass2 = reader.GetValue<SimpleClass>("cls2");
                str.NullableEnumField = reader.GetValue<TypeCode?>("ne");
                return str;
            }
        }

        [MetaSerializer(typeof(SimpleClassSerializer))]
        public class SimpleClass : IEquatable<SimpleClass>
        {
            public int X { get; set; }

            public SimpleClass( int x )
            {
                X = x;
            }

            public bool Equals( SimpleClass other )
            {
                if ( ReferenceEquals( null, other ) ) return false;
                if ( ReferenceEquals( this, other ) ) return true;
                return X == other.X;
            }

            public override bool Equals( object obj )
            {
                if ( ReferenceEquals( null, obj ) ) return false;
                if ( ReferenceEquals( this, obj ) ) return true;
                if ( obj.GetType() != this.GetType() ) return false;
                return Equals( (SimpleClass) obj );
            }

            public override int GetHashCode()
            {
                return X;
            }
        }

        public class SimpleClassSerializer : ReferenceTypeSerializer<SimpleClass>
        {
            public override void SerializeObject( SimpleClass obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
            {
                constructorArguments.SetValue( "x", obj.X );
            }

            public override void DeserializeFields( SimpleClass obj, IArgumentsReader initializationArguments )
            { }

            public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
            {
                return new SimpleClass(constructorArguments.GetValue<int>("x"));   
            }
        }

        #endregion
    }
}