// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.CompileTime.Serialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serialization
{
    public class ReflectionTypesSerializationTests : SerializationTestsBase
    {
        [Fact]
        public void TestTypeClass()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( typeof( DateTime ) );
            this.TestSerialization( typeof( Guid ) );
            this.TestSerialization( typeof( IntrinsicSerializationTests ) );
After:
            SerializationTestsBase.TestSerialization( typeof( DateTime ) );
            SerializationTestsBase.TestSerialization( typeof( Guid ) );
            SerializationTestsBase.TestSerialization( typeof( IntrinsicSerializationTests ) );
*/
            TestSerialization( typeof(DateTime) );
            TestSerialization( typeof(Guid) );
            TestSerialization( typeof(IntrinsicSerializationTests) );
        }

        [Fact]
        public void TestTypeGenericClosed()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( typeof( Dictionary<string, string> ) );
After:
            SerializationTestsBase.TestSerialization( typeof( Dictionary<string, string> ) );
*/
            TestSerialization( typeof(Dictionary<string, string>) );
        }

        [Fact]
        public void TestTypeGenericOpen()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( typeof( Dictionary<,> ) );
After:
            SerializationTestsBase.TestSerialization( typeof( Dictionary<,> ) );
*/
            TestSerialization( typeof(Dictionary<,>) );
        }

        [Fact]
        public void TestTypeGenericTypeParameter()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( typeof( Dictionary<,> ).GetGenericArguments()[0] );
After:
            SerializationTestsBase.TestSerialization( typeof( Dictionary<,> ).GetGenericArguments()[0] );
*/
            TestSerialization( typeof(Dictionary<,>).GetGenericArguments()[0] );
        }

#pragma warning disable SA1401 // Fields should be private
        public int TestField;
#pragma warning restore SA1401 // Fields should be private

        public int TestProperty { get; set; }

        // TODO: Other, more esoteric reflection objects: generic parameters, method arguments etc.

        [Fact]
        public void TestTypeIntrinsics()
        {
/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            this.TestSerialization( typeof( byte ) );
            this.TestSerialization( typeof( sbyte ) );
            this.TestSerialization( typeof( short ) );
            this.TestSerialization( typeof( ushort ) );
            this.TestSerialization( typeof( int ) );
            this.TestSerialization( typeof( uint ) );
            this.TestSerialization( typeof( long ) );
            this.TestSerialization( typeof( ulong ) );
            this.TestSerialization( typeof( float ) );
            this.TestSerialization( typeof( double ) );
            this.TestSerialization( typeof( string ) );
            this.TestSerialization( typeof( DottedString ) );
            this.TestSerialization( typeof( char ) );
            this.TestSerialization( typeof( object ) );
            this.TestSerialization( typeof( void ) );
            this.TestSerialization( typeof( Type ) );
            this.TestSerialization( typeof( ValueType ) );
After:
            SerializationTestsBase.TestSerialization( typeof( byte ) );
            SerializationTestsBase.TestSerialization( typeof( sbyte ) );
            SerializationTestsBase.TestSerialization( typeof( short ) );
            SerializationTestsBase.TestSerialization( typeof( ushort ) );
            SerializationTestsBase.TestSerialization( typeof( int ) );
            SerializationTestsBase.TestSerialization( typeof( uint ) );
            SerializationTestsBase.TestSerialization( typeof( long ) );
            SerializationTestsBase.TestSerialization( typeof( ulong ) );
            SerializationTestsBase.TestSerialization( typeof( float ) );
            SerializationTestsBase.TestSerialization( typeof( double ) );
            SerializationTestsBase.TestSerialization( typeof( string ) );
            SerializationTestsBase.TestSerialization( typeof( DottedString ) );
            SerializationTestsBase.TestSerialization( typeof( char ) );
            SerializationTestsBase.TestSerialization( typeof( object ) );
            SerializationTestsBase.TestSerialization( typeof( void ) );
            SerializationTestsBase.TestSerialization( typeof( Type ) );
            SerializationTestsBase.TestSerialization( typeof( ValueType ) );
*/
            TestSerialization( typeof(byte) );
            TestSerialization( typeof(sbyte) );
            TestSerialization( typeof(short) );
            TestSerialization( typeof(ushort) );
            TestSerialization( typeof(int) );
            TestSerialization( typeof(uint) );
            TestSerialization( typeof(long) );
            TestSerialization( typeof(ulong) );
            TestSerialization( typeof(float) );
            TestSerialization( typeof(double) );
            TestSerialization( typeof(string) );
            TestSerialization( typeof(DottedString) );
            TestSerialization( typeof(char) );
            TestSerialization( typeof(object) );
            TestSerialization( typeof(void) );
            TestSerialization( typeof(Type) );
            TestSerialization( typeof(ValueType) );
        }

        public class ReflectionTestClass
        {
            public bool MethodInvoked { get; set; }

            public void Method()
            {
                this.MethodInvoked = true;
            }

#pragma warning disable CA1822 // Mark members as static
            public void MethodWithParameter( int parameter )
#pragma warning restore CA1822 // Mark members as static
            { }
        }
    }
}