// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serializers
{
    public class InheritanceTests : SerializerTestBase
    {
        [Fact]
        public void BaseSerializerInTheSameAssembly()
        {
            // Verifies that ILamaSerializable compile-time type with explicit parameterless constructor can be serialized and deserialized.
            // Generator should not inject parameterless constructor when it is already defined.
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    public int BaseField;
}
public class B : A
{
    public int DerivedField;
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "B" );
            var serializer = GetSerializer( type );

            dynamic instance = Activator.CreateInstance( type )!;
            instance.BaseField = 13;
            instance.DerivedField = 42;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            serializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = serializer.CreateInstance( type, constructorArgumentsReader );
            serializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.BaseField );
            Assert.Equal( 42, deserialized.DerivedField );
        }

        [Fact]
        public void AbstractBase()
        {
            // Verifies that IMetaSerializable readonly struct type can be serialized and deserialized (round-trip).
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public abstract class A : ILamaSerializable
{
    public int PropertyA { get; }

    public A(int propertyA)
    {
        this.PropertyA = propertyA;
    }
}
public class B : A
{
    public int PropertyB { get; }

    public B(int propertyA, int propertyB) : base(propertyA)
    {
        this.PropertyB = propertyB;
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var typeB = project.GetType( "B" );
            var metaSerializer = GetSerializer( typeB );

            dynamic instance = Activator.CreateInstance( typeB, 13, 42 )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( typeB, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.PropertyA );
            Assert.Equal( 42, deserialized.PropertyB );
        }
    }
}