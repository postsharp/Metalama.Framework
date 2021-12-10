// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.LamaSerialization;
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
            TestSerialization( typeof(DateTime) );
            TestSerialization( typeof(Guid) );
            TestSerialization( typeof(IntrinsicSerializationTests) );
        }

        [Fact]
        public void TestTypeGenericClosed()
        {
            TestSerialization( typeof(Dictionary<string, string>) );
        }

        [Fact]
        public void TestTypeGenericOpen()
        {
            TestSerialization( typeof(Dictionary<,>) );
        }

        [Fact]
        public void TestTypeGenericTypeParameter()
        {
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