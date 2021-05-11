// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaMethodInfoTests : SerializerTestsBase
    {
        [Fact]
        public void TestSerializationOfMethod()
        {
            var code = "class Target { public static int Method() => 42; }";
            var serialized = this.SerializeTargetDotMethod( code );

            Assert.Equal(
                "((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method~System.Int32\")))",
                serialized );

            TestExpression<MethodInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target", info.DeclaringType!.Name );
                    Assert.Equal( "Method", info.Name );
                    Assert.Equal( 42, info.Invoke( null, new object[0] ) );
                } );
        }

        [Fact]
        public void TestGenericMethod()
        {
            var code = "class Target { public static T Method<T>(T a) => (T)(object)(2*(int)(object)a); }";
            var serialized = this.SerializeTargetDotMethod( code );

            Assert.Equal(
                "((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method``1(``0)~``0\")))",
                serialized );

            TestExpression<MethodInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target", info.DeclaringType!.Name );
                    Assert.Equal( "Method", info.Name );
                    Assert.Equal( 42, info.MakeGenericMethod( typeof(int) ).Invoke( null, new object[] { 21 } ) );
                } );
        }

        private string SerializeTargetDotMethod( string code )
        {
            var compilation = CreateCompilationModel( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Methods.Single( m => m.Name == "Method" );
            var method = (Method) single;
            var actual = this.Serialize( CompileTimeMethodInfo.Create( method ) ).ToString();

            return actual;
        }
    }
}