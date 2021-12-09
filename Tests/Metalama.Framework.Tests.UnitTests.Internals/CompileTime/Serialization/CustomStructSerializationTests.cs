// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Serialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public class CustomStructSerializationTests : SerializationTestsBase
    {
        [Fact]
        public void SerializeStruct_BasicTest()
        {
            var s = new SimpleStruct( 1, DateTime.Now ) { StringValue = "Test", NullableEnumField = TypeCode.Char };

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( s );
After:
            SerializationTestsBase.TestSerialization( s );
*/
            TestSerialization( s );
        }

        [Fact]
        public void SerializeStruct_BoxedStruct()
        {
            var s = new SimpleStruct( 1, DateTime.Now ) { StringValue = "Test" };

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( (object) s );
After:
            SerializationTestsBase.TestSerialization( (object) s );
*/
            TestSerialization( (object) s );
        }

        [Fact]
        public void SerializeStruct_GenericStruct()
        {
            var s = new GenericStruct<string> { Value = "1" };

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( s );
After:
            SerializationTestsBase.TestSerialization( s );
*/
            TestSerialization( s );
        }

        [Fact]
        public void SerializeStruct_NestedStructs()
        {
            var s = new GenericStruct<SimpleStruct> { Value = new SimpleStruct( 5, DateTime.MinValue ) };

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( s );
After:
            SerializationTestsBase.TestSerialization( s );
*/
            TestSerialization( s );
        }

        [Fact]
        public void SerializeStruct_WithObjectReferences()
        {
            var cls = new SimpleClass( 11 );
            var str = new SimpleStruct( 1, DateTime.Now ) { SimpleClass = cls, SimpleClass2 = cls };

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            var str2 = this.TestSerialization( str );
After:
            var str2 = SerializationTestsBase.TestSerialization( str );
*/
            var str2 = TestSerialization( str );
            Assert.Same( str2.SimpleClass, str2.SimpleClass2 );
        }

        [Serializer( typeof(GenericStructSerializer<>) )]
        public struct GenericStruct<T> : IEquatable<GenericStruct<T>>
            where T : notnull
        {
            public T Value { get; set; }

            public string? StringValue { get; set; }

            public bool Equals( GenericStruct<T> other )
            {
                return EqualityComparer<T>.Default.Equals( this.Value, other.Value ) && StringComparer.Ordinal.Equals( this.StringValue, other.StringValue );
            }

            public override bool Equals( object? obj )
            {
                if ( ReferenceEquals( null, obj ) )
                {
                    return false;
                }

                return obj is GenericStruct<T> @struct && this.Equals( @struct );
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (EqualityComparer<T>.Default.GetHashCode( this.Value ) * 397)
                           ^ (this.StringValue != null ? StringComparer.Ordinal.GetHashCode( this.StringValue ) : 0);
                }
            }
        }

        public class GenericStructSerializer<T> : ValueTypeSerializer<GenericStruct<T>>
            where T : notnull
        {
            public override void SerializeObject( GenericStruct<T> obj, IArgumentsWriter constructorArguments )
            {
                constructorArguments.SetValue( "T", obj.Value );
                constructorArguments.SetValue( "s", obj.StringValue );
            }

            public override GenericStruct<T> DeserializeObject( IArgumentsReader constructorArguments )
            {
                // Assertion on nullability was added after the code import from PostSharp.
                var s = new GenericStruct<T> { StringValue = constructorArguments.GetValue<string>( "s" ), Value = constructorArguments.GetValue<T?>( "T" )! };

                return s;
            }
        }

        [Serializer( typeof(SimpleStructSerializer) )]
        public struct SimpleStruct : IEquatable<SimpleStruct>
        {
            public int IntValue { get; set; }

            public string? StringValue { get; set; }

            public DateTime TimeValue { get; set; }

            public TypeCode? NullableEnumField { get; set; }

            public SimpleClass? SimpleClass { get; set; }

            public SimpleClass? SimpleClass2 { get; set; }

            public SimpleStruct( int intValue, DateTime timeValue ) : this()
            {
                this.IntValue = intValue;
                this.TimeValue = timeValue;
            }

            public bool Equals( SimpleStruct other )
            {
                return this.IntValue == other.IntValue && StringComparer.Ordinal.Equals( this.StringValue, other.StringValue )
                                                       && this.TimeValue.Equals( other.TimeValue ) && Equals( this.SimpleClass, other.SimpleClass ) &&
                                                       Equals( this.SimpleClass2, other.SimpleClass2 ) && Equals(
                                                           this.NullableEnumField,
                                                           other.NullableEnumField );
            }

            public override bool Equals( object? obj )
            {
                if ( ReferenceEquals( null, obj ) )
                {
                    return false;
                }

                return obj is SimpleStruct @struct && this.Equals( @struct );
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = this.IntValue;
                    hashCode = (hashCode * 397) ^ (this.StringValue != null ? StringComparer.Ordinal.GetHashCode( this.StringValue ) : 0);
                    hashCode = (hashCode * 397) ^ this.TimeValue.GetHashCode();
                    hashCode = (hashCode * 397) ^ (this.SimpleClass != null ? this.SimpleClass.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.SimpleClass2 != null ? this.SimpleClass2.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (this.NullableEnumField != null ? this.NullableEnumField.GetHashCode() : 0);

                    return hashCode;
                }
            }
        }

        public class SimpleStructSerializer : ValueTypeSerializer<SimpleStruct>
        {
            public override void SerializeObject( SimpleStruct obj, IArgumentsWriter constructorArguments )
            {
                constructorArguments.SetValue( "int", obj.IntValue );
                constructorArguments.SetValue( "time", obj.TimeValue );
                constructorArguments.SetValue( "string", obj.StringValue );
                constructorArguments.SetValue( "cls", obj.SimpleClass );
                constructorArguments.SetValue( "cls2", obj.SimpleClass2 );
                constructorArguments.SetValue( "ne", obj.NullableEnumField );
            }

            public override SimpleStruct DeserializeObject( IArgumentsReader constructorArguments )
            {
                var str = new SimpleStruct( constructorArguments.GetValue<int>( "int" ), constructorArguments.GetValue<DateTime>( "time" ) )
                {
                    StringValue = constructorArguments.GetValue<string>( "string" ),
                    SimpleClass = constructorArguments.GetValue<SimpleClass>( "cls" ),
                    SimpleClass2 = constructorArguments.GetValue<SimpleClass>( "cls2" ),
                    NullableEnumField = constructorArguments.GetValue<TypeCode?>( "ne" )
                };

                return str;
            }
        }

        [Serializer( typeof(SimpleClassSerializer) )]
        public class SimpleClass : IEquatable<SimpleClass>
        {
            public int X { get; set; }

            public SimpleClass( int x )
            {
                this.X = x;
            }

            public bool Equals( SimpleClass? other )
            {
                if ( ReferenceEquals( null, other ) )
                {
                    return false;
                }

                if ( ReferenceEquals( this, other ) )
                {
                    return true;
                }

                return this.X == other.X;
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

                return this.Equals( (SimpleClass) obj );
            }

            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return this.X;
            }
        }

        public class SimpleClassSerializer : ReferenceTypeSerializer<SimpleClass>
        {
            public override void SerializeObject( SimpleClass obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
            {
                constructorArguments.SetValue( "x", obj.X );
            }

            public override void DeserializeFields( SimpleClass obj, IArgumentsReader initializationArguments ) { }

            public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
            {
                return new SimpleClass( constructorArguments.GetValue<int>( "x" ) );
            }
        }
    }
}