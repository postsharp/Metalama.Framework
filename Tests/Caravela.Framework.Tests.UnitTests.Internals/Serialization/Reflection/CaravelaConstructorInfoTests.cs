// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaConstructorInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestConstructor()
        {
            var code = "class Target { public Target(int hello) { } }";
            var serialized = this.SerializeConstructor( code );

            this.AssertEqual(
                @"((global::System.Reflection.ConstructorInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.#ctor(System.Int32)"")))",
                serialized );

            TestExpression<ConstructorInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target", info.DeclaringType!.Name );
                    Assert.Single( info.GetParameters() );
                } );
        }

        [Fact]
        public void TestGenericConstructor()
        {
            var code = "class Target<T> where T: struct { public Target(T hello) { } }";
            var serialized = this.SerializeConstructor( code );

            this.AssertEqual(
                @"((global::System.Reflection.ConstructorInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.#ctor(`0)""), typeof(global::Target<>).TypeHandle))",
                serialized );

            TestExpression<ConstructorInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target`1", info.DeclaringType!.Name );
                    Assert.Single( info.GetParameters() );
                } );
        }

        [Fact]
        public void TestDefaultConstructor()
        {
            var code = "class Target {  }";
            var serialized = this.SerializeConstructor( code );

            this.AssertEqual(
                @"((global::System.Reflection.ConstructorInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.#ctor"")))",
                serialized );

            TestExpression<ConstructorInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target", info.DeclaringType!.Name );
                    Assert.Empty( info.GetParameters() );
                } );
        }

        // If there is no constructor, there is no constructor to serialize. We are at C#, not IL level.
        private string SerializeConstructor( string code )
        {
            using var testContext = this.CreateTestContext();

            var compilation = testContext.CreateCompilationModel( code );
            var namedTypes = compilation.Types;
            var type = namedTypes.Single( t => t.Name == "Target" );
            var methods = type.Constructors;
            var single = methods.Single();
            var p = (single as Constructor)!;
            var actual = testContext.Serialize( CompileTimeConstructorInfo.Create( p ) ).ToString();

            return actual;
        }

        public CaravelaConstructorInfoTests( ITestOutputHelper helper ) : base( helper ) { }
    }
}