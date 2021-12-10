﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serializers
{
    public class ManualSerializerTests : SerializerTestBase
    {
        [Fact]
        public void CustomBaseSerializer()
        {
            // Verifies that ILamaSerializable compile-time type with explicit parameterless constructor can be serialized and deserialized.
            // Generator should not inject parameterless constructor when it is already defined.
            var code = @"
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
[Serializer(typeof(CustomSerializer))]
public class A : ILamaSerializable
{
    public int BaseField;

    public class CustomSerializer : ReferenceTypeSerializer
    {
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
            var o = new A();
            o.BaseField = constructorArguments.GetValue<int>(""BaseField"");
            return o;
        }

        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var a = (A)obj;
            constructorArguments.SetValue(""BaseField"",a.BaseField);
        }

        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
        }
    }
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
    }
}