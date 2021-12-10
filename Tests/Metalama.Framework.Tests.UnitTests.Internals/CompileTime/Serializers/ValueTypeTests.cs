// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serializers
{
    public class ValueTypeTests : SerializerTestBase
    {
        [Fact]
        public void SimpleStruct()
        {
            // Verifies that IMetaSerializable readonly struct type can be serialized and deserialized (round-trip).
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public struct A : ILamaSerializable
{
    public readonly int Field;
    public int Property { get; }
    public int MutableProperty { get; set; }

    public A(int field, int property)
    {
        this.Field = field;
        this.Property = property;
        this.MutableProperty = 0;
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project!.GetType( "A" );
            var metaSerializer = GetSerializer( type );

            dynamic instance = Activator.CreateInstance( type, 13, 27 )!;
            instance.MutableProperty = 42;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( type, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.Field );
            Assert.Equal( 27, deserialized.Property );
            Assert.Equal( 42, deserialized.MutableProperty );
        }

        [Fact]
        public void ReadonlyStruct()
        {
            // Verifies that IMetaSerializable readonly struct type can be serialized and deserialized (round-trip).
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public readonly struct A : ILamaSerializable
{
    public readonly int Field;
    public int Property { get; }

    public A(int field, int property)
    {
        this.Field = field;
        this.Property = property;
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project!.GetType( "A" );
            var metaSerializer = GetSerializer( type );

            dynamic instance = Activator.CreateInstance( type, 13, 42 )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( type, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.Field );
            Assert.Equal( 42, deserialized.Property );
        }
    }
}