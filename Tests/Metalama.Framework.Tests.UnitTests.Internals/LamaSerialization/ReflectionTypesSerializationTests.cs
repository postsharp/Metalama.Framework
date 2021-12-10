﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.LamaSerialization;
using System;
using System.Collections.Generic;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.LamaSerialization
{
    public class ReflectionTypesSerializationTests : SerializationTestsBase
    {
        [Fact]
        public void TestTypeClass()
        {
            this.TestSerialization( typeof(DateTime) );
            this.TestSerialization( typeof(Guid) );
            this.TestSerialization( typeof(IntrinsicSerializationTests) );
        }

        [Fact]
        public void TestTypeGenericClosed()
        {
            this.TestSerialization( typeof(Dictionary<string, string>) );
        }

        [Fact]
        public void TestTypeGenericOpen()
        {
            this.TestSerialization( typeof(Dictionary<,>) );
        }

        [Fact]
        public void TestTypeGenericTypeParameter()
        {
            this.TestSerialization( typeof(Dictionary<,>).GetGenericArguments()[0] );
        }

#pragma warning disable SA1401 // Fields should be private
        public int TestField;
#pragma warning restore SA1401 // Fields should be private

        public int TestProperty { get; set; }

        // TODO: Other, more esoteric reflection objects: generic parameters, method arguments etc.

        [Fact]
        public void TestTypeIntrinsics()
        {
            this.TestSerialization( typeof(byte) );
            this.TestSerialization( typeof(sbyte) );
            this.TestSerialization( typeof(short) );
            this.TestSerialization( typeof(ushort) );
            this.TestSerialization( typeof(int) );
            this.TestSerialization( typeof(uint) );
            this.TestSerialization( typeof(long) );
            this.TestSerialization( typeof(ulong) );
            this.TestSerialization( typeof(float) );
            this.TestSerialization( typeof(double) );
            this.TestSerialization( typeof(string) );
            this.TestSerialization( typeof(DottedString) );
            this.TestSerialization( typeof(char) );
            this.TestSerialization( typeof(object) );
            this.TestSerialization( typeof(void) );
            this.TestSerialization( typeof(Type) );
            this.TestSerialization( typeof(ValueType) );
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