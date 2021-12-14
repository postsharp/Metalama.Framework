// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.GeneratedSerializers
{
    public class NonSerializedTests : SerializerTestBase
    {
        [Fact]
        public void Field()
        {
            // Verifies that serializable type with non-serialized field can be serialized and deserialized.
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    [LamaNonSerialized]
    public int Field;
    public int Property { get; set; }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
            var lamaSerializer = GetSerializer( type );

            dynamic instance = Activator.CreateInstance( type )!;
            instance.Field = 13;
            instance.Property = 42;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            lamaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = lamaSerializer.CreateInstance( type, constructorArgumentsReader );
            lamaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 0, deserialized.Field );
            Assert.Equal( 42, deserialized.Property );
        }

        [Fact]
        public void Property ()
        {
            // Verifies that serializable type with non-serialized property can be serialized and deserialized.
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    public int Field;
    [LamaNonSerialized]
    public int Property { get; set; }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
            var lamaSerializer = GetSerializer( type );

            dynamic instance = Activator.CreateInstance( type )!;
            instance.Field = 13;
            instance.Property = 42;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            lamaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = lamaSerializer.CreateInstance( type, constructorArgumentsReader );
            lamaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.Field );
            Assert.Equal( 0, deserialized.Property );
        }

        [Fact]
        public void PropertyBackingField()
        {
            // Verifies that serializable type with non-serialized auto-property backing field can be serialized and deserialized.
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    public int Field;
    [field:LamaNonSerialized]
    public int Property { get; set; }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
            var lamaSerializer = GetSerializer( type );

            dynamic instance = Activator.CreateInstance( type )!;
            instance.Field = 13;
            instance.Property = 42;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            lamaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = lamaSerializer.CreateInstance( type, constructorArgumentsReader );
            lamaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.Field );
            Assert.Equal( 0, deserialized.Property );
        }
    }
}