// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaNestedTests : ReflectionTestBase
    {
        [Fact]
        public void TestType()
        {
            var code = "class Target { class Sub { }  }";
            var serialized = this.SerializeType( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target.Sub""))", serialized );

            TestExpression<Type>( code, serialized, ( info ) => Assert.Equal( "Sub", info.Name ) );
        }

        private string SerializeType( string code )
        {
            var compilation = CreateCompilation( code );
            IType single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).NestedTypes.Single( nt => nt.Name == "Sub" );
            return new CaravelaTypeSerializer().Serialize( CompileTimeType.Create( single ) ).ToString();
        }

        public CaravelaNestedTests( ITestOutputHelper helper ) : base( helper )
        {
        }
    }
}