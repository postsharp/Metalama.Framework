// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaGenericInstancesTests : ReflectionTestBase
    {
        public CaravelaGenericInstancesTests( ITestOutputHelper helper ) : base( helper ) { }

        [Fact]
        public void TestListString()
        {
            this.AssertFieldType(
                "class Outer { class Inner { System.Collections.Generic.List<string> Target; } }",
                typeof( List<string> ),
                @"typeof(global::System.Collections.Generic.List<global::System.String>)" );
        }

        [Fact]
        public void TestDictionaryString()
        {
            this.AssertFieldType(
                "class Outer { class Inner { System.Collections.Generic.Dictionary<string[],int?> Target; } }",
                typeof( Dictionary<string[], int?> ),
                @"typeof(global::System.Collections.Generic.Dictionary<global::System.String[],global::System.Int32?>)" );
        }

        private void AssertFieldType( string code, Type expectedType, string expected )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var allTypes = testContext.Compilation.Types;
            var nestedTypes = allTypes.Single().NestedTypes;
            var innerType = nestedTypes.Single();
            var allProperties = innerType.Fields;

            var serialized = testContext.Serialize( CompileTimeType.Create( allProperties.Single().Type ) )
                .ToString();

            TestExpression<Type>( code, serialized, info => Assert.Equal( expectedType, info ) );

            this.AssertEqual( expected, serialized );
        }
    }
}