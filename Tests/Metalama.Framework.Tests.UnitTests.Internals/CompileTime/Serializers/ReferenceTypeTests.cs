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

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
            var metaSerializer = GetSerializer( type );

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

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
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

            var project = CreateCompileTimeProject( domain, testContext, code );

            var typeA = project.GetType( "A" );
            var typeB = project.GetType( "B" );
            var metaSerializer = GetSerializer( typeA );

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
            var metaSerializer = GetSerializer( typeA );

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

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A" );
            var metaSerializer = GetSerializer( type );

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
        public void GenericClass()
        {
            // Verifies that IMetaSerializable compile-time type with explicit parameterless constructor can be serialized and deserialized.
            // Generator should not inject parameterless constructor when it is already defined.
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A<T> : ILamaSerializable
{
    public T Property { get; }

    public A(T property)
    {
        this.Property = property;
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "A`1" ).MakeGenericType( typeof(int) );
            var metaSerializer = GetSerializer( type );

            dynamic instance = Activator.CreateInstance( type, 42 )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( type, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 42, deserialized.Property );
        }

        [Fact]
        public void ClosedGenericValue()
        {
            // Verifies that generated serializer correctly handles a field/property of closed generic type
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A<T>
{
    public T Property { get; set; }

    public A(T property)
    {
        this.Property = property;
    }
}

public class B : ILamaSerializable
{
    public A<object> Field;
    public A<int> Property { get; set; }

    public B(A<object> field, A<int> property)
    {
        this.Field = field;
        this.Property = property;
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var typeAObject = project.GetType( "A`1" ).MakeGenericType( typeof(object) );
            var typeAInt = project.GetType( "A`1" ).MakeGenericType( typeof(int) );
            var typeB = project.GetType( "B" );
            var metaSerializer = GetSerializer( typeB );

            var obj = new object();
            dynamic instanceAObj = Activator.CreateInstance( typeAObject, obj )!;
            dynamic instanceAInt = Activator.CreateInstance( typeAInt, 42 )!;
            var instanceB = Activator.CreateInstance( typeB, instanceAObj, instanceAInt )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instanceB, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( typeB, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Same( instanceAObj, deserialized.Field );
            Assert.Same( instanceAInt, deserialized.Property );
        }

        [Fact]
        public void OpenGenericValue()
        {
            // Verifies that generated serializer correctly handles a field/property of closed generic type
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A<T>
{
    public T Property { get; set; }

    public A(T property)
    {
        this.Property = property;
    }
}

public class B<T> : ILamaSerializable
{
    public A<T> Property { get; set; }

    public B(A<T> property)
    {
        this.Property = property;
    }
}
";

            using var domain = new UnloadableCompileTimeDomain();
            using var testContext = this.CreateTestContext();

            var project = CreateCompileTimeProject( domain, testContext, code );

            var typeA = project.GetType( "A`1" ).MakeGenericType( typeof(int) );
            var typeB = project.GetType( "B`1" ).MakeGenericType( typeof(int) );
            var metaSerializer = GetSerializer( typeB );

            dynamic instanceA = Activator.CreateInstance( typeA, 42 )!;
            var instanceB = Activator.CreateInstance( typeB, instanceA )!;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instanceB, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( typeB, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Same( instanceA, deserialized.Property );
        }
    }
}