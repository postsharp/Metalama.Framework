// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaGenericInstancesTests : ReflectionTestBase
    {
        private readonly ObjectSerializers _objectSerializers;

        public CaravelaGenericInstancesTests( ITestOutputHelper helper ) : base( helper )
        {
            this._objectSerializers = new ObjectSerializers();
        }

        [Fact]
        public void TestListString()
        {
            this.AssertFieldType(
                "class Outer { class Inner { System.Collections.Generic.List<string> Target; } }",
                typeof( List<string> ),
                @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Collections.Generic.List`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")))" );
        }

        [Fact]
        public void TestDictionaryString()
        {
            this.AssertFieldType(
                "class Outer { class Inner { System.Collections.Generic.Dictionary<string[],int?> Target; } }",
                typeof( Dictionary<string[], int?> ),
                @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Collections.Generic.Dictionary`2"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")).MakeArrayType(), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Nullable`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))))" );
        }

        private void AssertFieldType( string code, Type expectedType, string expected )
        {
            var allTypes = CreateCompilation( code ).DeclaredTypes;
            var nestedTypes = allTypes.Single().NestedTypes;
            var innerType = nestedTypes.Single();
            var allProperties = innerType.Properties;
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression(
                    CompileTimeType.Create(
                        allProperties.Single().Type ) )
                .ToString();

            TestExpression<Type>( code, serialized, ( info ) => Assert.Equal( expectedType, info ) );
            Assert.Equal( expected, serialized );
        }
    }
}