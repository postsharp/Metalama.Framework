// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.TestFramework;
using System;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime.Serializers
{
    public class InheritanceTests : MetaSerializerTestBase
    {
        [Fact]
        public void BaseSerializerInTheSameAssembly()
        {
            // Verifies that IMetaSerializable compile-time type with explicit parameterless constructor can be serialized and deserialized.
            // Generator should not inject parameterless constructor when it is already defined.
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Serialization;
[assembly: CompileTime]
public class A : IMetaSerializable
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

/* Unmerged change from project 'Metalama.Framework.Tests.UnitTests.Internals (netframework4.8)'
Before:
            var project = this.CreateCompileTimeProject( domain, testContext, code );
After:
            var project = MetaSerializerTestBase.CreateCompileTimeProject( domain, testContext, code );
*/
            var project = CreateCompileTimeProject( domain, testContext, code );

            var type = project.GetType( "B" );
            var metaSerializer = GetMetaSerializer( type );

            dynamic instance = Activator.CreateInstance( type )!;
            instance.BaseField = 13;
            instance.DerivedField = 42;

            var constructorArgumentsWriter = new TestArgumentsWriter();
            var initializationArgumentsWriter = new TestArgumentsWriter();
            metaSerializer.SerializeObject( instance, constructorArgumentsWriter, initializationArgumentsWriter );

            var constructorArgumentsReader = constructorArgumentsWriter.ToReader();
            var initializationArgumentsReader = initializationArgumentsWriter.ToReader();

            dynamic deserialized = metaSerializer.CreateInstance( type, constructorArgumentsReader );
            metaSerializer.DeserializeFields( ref deserialized, initializationArgumentsReader );

            Assert.Equal( 13, deserialized.BaseField );
            Assert.Equal( 42, deserialized.DerivedField );
        }
    }
}