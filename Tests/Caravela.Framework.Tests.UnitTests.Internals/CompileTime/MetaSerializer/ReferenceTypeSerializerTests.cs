// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework;
using System;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime.MetaSerialization
{
    public class ReferenceTypeSerializerTests : MetaSerializerTestBase
    {
        // Tests verify correctness of the generated serializer.

        [Fact]
        public void MutableMembers()
        {
            // Verifies that IMetaSerializable type with mutable members can be serialized and deserialized (round-trip).
            var code = @"
using Caravela.Framework.Aspects;
using Caravela.Framework.Serialization;
[assembly: CompileTime]
public class A : IMetaSerializable
{
    public int Field;
    public int Property { get; set; }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();
            var project = this.CreateCompileTimeProject( domain, testContext, code );

            var type = project!.GetType( "A" );
            var metaSerializer = GetMetaSerializer( type );

            dynamic instance = Activator.CreateInstance(type)!;
            instance.Field = 13;
            instance.Property = 42;

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

        [Fact]
        public void ReadOnlyMembers()
        {
            // Verifies that IMetaSerializable type with read-only members can be serialized and deserialized (round-trip).
            var code = @"
using Caravela.Framework.Aspects;
using Caravela.Framework.Serialization;
[assembly: CompileTime]
public class A : IMetaSerializable
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
            var project = this.CreateCompileTimeProject( domain, testContext, code );

            var type = project!.GetType( "A" );
            var metaSerializer = GetMetaSerializer( type );

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

        [Fact]
        public void ExplicitParameterlessConstructor()
        {
            // Verifies that IMetaSerializable compile-time type with explicit parameterless constructor can be serialized and deserialized.
            // Generator should not inject parameterless constructor when it is already defined.
            var code = @"
using Caravela.Framework.Aspects;
using Caravela.Framework.Serialization;
[assembly: CompileTime]
public class A : IMetaSerializable
{
    public A()
    {
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();
            var project = this.CreateCompileTimeProject( domain, testContext, code );

            var type = project!.GetType( "A" );
            var metaSerializer = GetMetaSerializer( type );

            dynamic instance = Activator.CreateInstance( type )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( type, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );
        }
    }
}