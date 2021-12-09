// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serializers
{
    public class ReferenceTypeTests : SerializerTestBase
    {
        // Tests verify correctness of the generated serializer.

        [Fact]
        public void MutableMembers()
        {
            // Verifies that ILamaSerializable type with mutable members can be serialized and deserialized (round-trip).
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    public int Field;
    public int Property { get; set; }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            var project = this.CreateCompileTimeProject( domain, testContext, code );
After:
            var project = MetaSerializerTestBase.CreateCompileTimeProject( domain, testContext, code );
*/
            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
            var metaSerializer = GetMetaSerializer( type );

            dynamic instance = Activator.CreateInstance( type )!;
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
        public void ReadOnlyValueTypeMembers()
        {
            // Verifies that ILamaSerializable type with read-only members can be serialized and deserialized (round-trip).
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
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

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            var project = this.CreateCompileTimeProject( domain, testContext, code );
After:
            var project = MetaSerializerTestBase.CreateCompileTimeProject( domain, testContext, code );
*/
            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
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
        public void ReadOnlyReferenceTypeMembers()
        {
            // Verifies that ILamaSerializable type with read-only members can be serialized and deserialized (round-trip).
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    public readonly B Field;
    public B Property { get; }

    public A(B field, B property)
    {
        this.Field = field;
        this.Property = property;
    }
}

public class B
{
    public readonly int Field;

    public B(int field)
    {
        this.Field = field;
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            /* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
            Before:
                        var project = this.CreateCompileTimeProject( domain, testContext, code );
            After:
                        var project = MetaSerializerTestBase.CreateCompileTimeProject( domain, testContext, code );
            */
            var project = CreateCompileTimeProject( domain, testContext, code );

            var typeA = project.GetType( "A" );
            var typeB = project.GetType( "B" );
            var metaSerializer = GetMetaSerializer( typeA );

            dynamic instanceB1 = Activator.CreateInstance( typeB, 13 )!;
            dynamic instanceB2 = Activator.CreateInstance( typeB, 42 )!;
            var instanceA = Activator.CreateInstance( typeA, instanceB1, instanceB2 )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instanceA, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( typeA, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.Field.Field );
            Assert.Equal( 42, deserialized.Property.Field );
        }

        [Fact]
        public void InitOnlyReferenceTypeMembers()
        {
            // Verifies that ILamaSerializable type with init-only members can be serialized and deserialized (round-trip).
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]

public class A : ILamaSerializable
{
    public B Property { get; init; }

    public A(B property)
    {
        this.Property = property;
    }
}

public class B
{
    public readonly int Field;

    public B(int field)
    {
        this.Field = field;
    }
}
";
            
#if NETFRAMEWORK
            code += "namespace System.Runtime.CompilerServices { internal static class IsExternalInit {}}";
#endif

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var typeA = project.GetType( "A" );
            var typeB = project.GetType( "B" );
            var metaSerializer = GetMetaSerializer( typeA );

            dynamic instanceB = Activator.CreateInstance( typeB, 42 )!;
            var instanceA = Activator.CreateInstance( typeA, instanceB )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instanceA, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( typeA, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 42, deserialized.Property.Field );
        }

        [Fact]
        public void ExplicitParameterlessConstructor()
        {
            // Verifies that ILamaSerializable compile-time type with explicit parameterless constructor can be serialized and deserialized.
            // Generator should not inject parameterless constructor when it is already defined.
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    public A()
    {
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            var project = this.CreateCompileTimeProject( domain, testContext, code );
After:
            var project = MetaSerializerTestBase.CreateCompileTimeProject( domain, testContext, code );
*/
            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
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

        [Fact]
        public void SimpleStruct()
        {
            // Verifies that ILamaSerializable readonly struct type can be serialized and deserialized (round-trip).
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

            /* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
            Before:
                        var project = this.CreateCompileTimeProject( domain, testContext, code );
            After:
                        var project = MetaSerializerTestBase.CreateCompileTimeProject( domain, testContext, code );
            */
            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
            var metaSerializer = GetMetaSerializer( type );

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
            // Verifies that ILamaSerializable readonly struct type can be serialized and deserialized (round-trip).
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

            /* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
            Before:
                        var project = this.CreateCompileTimeProject( domain, testContext, code );
            After:
                        var project = MetaSerializerTestBase.CreateCompileTimeProject( domain, testContext, code );
            */
            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
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
    }
}