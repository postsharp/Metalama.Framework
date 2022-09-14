// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Reflection
{
    public class MetalamaMethodInfoTests : SerializerTestsBase
    {
        [Fact]
        public void TestSerializationOfMethod()
        {
            var code = "class Target { public static int Method() => 42; }";
            var serialized = this.SerializeTargetDotMethod( code );

            Assert.Equal(
                @"((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Method()""))",
                serialized );

            this.TestExpression<MethodInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target", info.DeclaringType!.Name );
                    Assert.Equal( "Method", info.Name );
                    Assert.Equal( 42, info.Invoke( null, Array.Empty<object>() ) );
                } );
        }

        [Fact]
        public void TestGenericMethod()
        {
            var code = "class Target { public static T Method<T>(T a) => (T)(object)(2*(int)(object)a); }";
            var serialized = this.SerializeTargetDotMethod( code );

            Assert.Equal(
                @"((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Method<T>(T)""))",
                serialized );

            this.TestExpression<MethodInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target", info.DeclaringType!.Name );
                    Assert.Equal( "Method", info.Name );
                    Assert.Equal( 42, info.MakeGenericMethod( typeof(int) ).Invoke( null, new object[] { 21 } ) );
                } );
        }

        [Fact]
        public void TestMethodWithOutParameter()
        {
            var code = "class Target { public static int Method( out int x) { x = 100; return 42;  } }";
            var serialized = this.SerializeTargetDotMethod( code );

            Assert.Equal(
                @"((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Method(out int)""))",
                serialized );

            this.TestExpression<MethodInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "Target", info.DeclaringType!.Name );
                    Assert.Equal( "Method", info.Name );
                    Assert.Equal( 42, info.Invoke( null, Array.Empty<object>() ) );
                } );
        }

        private string SerializeTargetDotMethod( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var single = compilation.Types.Single( t => t.Name == "Target" ).Methods.Single( m => m.Name == "Method" );
            var method = (Method) single;
            var actual = testContext.Serialize( CompileTimeMethodInfo.Create( method ) ).ToString();

            return actual;
        }
    }
}