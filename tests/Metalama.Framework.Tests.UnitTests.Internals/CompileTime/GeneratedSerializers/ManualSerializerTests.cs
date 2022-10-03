﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.TestFramework;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.GeneratedSerializers
{
    public class ManualSerializerTests : SerializerTestBase
    {
        [Fact]
        public void CustomBaseSerializer()
        {
            // Verifies that custom serializer defined in the base type is correctly consumed by the generated serializer in the derived type.
            var code = @"
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : ILamaSerializable
{
    public int BaseField;

    public class CustomSerializer : ReferenceTypeSerializer
    {
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
            var o = new A();
            o.BaseField = constructorArguments.GetValue<int>(""BaseField"")-1;
            return o;
        }

        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var a = (A)obj;
            constructorArguments.SetValue(""BaseField"",a.BaseField+1);
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