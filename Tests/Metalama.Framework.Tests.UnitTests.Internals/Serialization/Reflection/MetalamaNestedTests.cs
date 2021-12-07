// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.ReflectionMocks;
using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class MetalamaNestedTests : ReflectionTestBase
    {
        [Fact]
        public void TestType()
        {
            var code = "class Target { class Sub { }  }";
            var serialized = this.SerializeType( code );

            this.AssertEqual(
                @"typeof(global::Target.Sub)",
                serialized );

            TestExpression<Type>( code, serialized, info => Assert.Equal( "Sub", info.Name ) );
        }

        private string SerializeType( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            IType single = compilation.Types.Single( t => t.Name == "Target" ).NestedTypes.Single( nt => nt.Name == "Sub" );

            return testContext.Serialize( CompileTimeType.Create( single ) ).ToString();
        }

        public MetalamaNestedTests( ITestOutputHelper helper ) : base( helper ) { }
    }
}