using System;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class ReflectionSerializationTests : TestBase
    {
        [Fact]
        void MethodHandleTest()
        {
            string code = @"
class C
{
    void M() {}
}";
            string expression = "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:C.M\"))";

            var methodInfo = (MethodInfo) ExecuteExpression( code, expression )!;

            Assert.NotNull( methodInfo );
        }

        [Fact]
        public void TestGenericMethod()
        {
            string code = "class Target { public static T Method<T>(T a) => (T)(object)(2*(int)(object)a); }";
            string serialized = "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method``1(``0)~``0\"))";
            var methodInfo = (MethodInfo) ExecuteExpression( code, serialized )!;
            Assert.Equal( 42, methodInfo.MakeGenericMethod( typeof( int ) ).Invoke( null, new object[] { 21 } ) );
        }

        [Fact]
        public void TestGenericType()
        {
            string code = "class Target<T> { }";
            string serialized = "System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(\"T:Target`1\"))";
            var type = (Type) ExecuteExpression( code, serialized )!;
            Assert.Equal( "Target`1", type.FullName );
        }
    }
}
