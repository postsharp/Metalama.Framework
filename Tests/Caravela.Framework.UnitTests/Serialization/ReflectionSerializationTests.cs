using System;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.UnitTests
{
    public class ReflectionSerializationTests : TestBase
    {
        [Fact]
        private void MethodHandleTest()
        {
            var code = @"
class C
{
    void M() {}
}";
            var expression = "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:C.M\"))";

            var methodInfo = (MethodInfo) ExecuteExpression( code, expression )!;

            Xunit.Assert.NotNull( methodInfo );
        }

        [Fact]
        public void TestGenericMethod()
        {
            var code = "class Target { public static T Method<T>(T a) => (T)(object)(2*(int)(object)a); }";
            var serialized = "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method``1(``0)~``0\"))";
            var methodInfo = (MethodInfo) ExecuteExpression( code, serialized )!;
            Xunit.Assert.Equal<object?>( 42, methodInfo.MakeGenericMethod( typeof( int ) ).Invoke( null, new object[] { 21 } ) );
        }

        [Fact]
        public void TestFieldInGenericType()
        {
            var code = "class Target<T> { int f; }";
            var serialized = @"
System.Reflection.FieldInfo.GetFieldFromHandle(
    Caravela.Compiler.Intrinsics.GetRuntimeFieldHandle(""F:Target`1.f""),
    Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1""))";
            var fieldInfo = (FieldInfo) ExecuteExpression( code, serialized )!;
            Xunit.Assert.Equal( "f", fieldInfo.Name );
        }

        [Fact]
        public void TestGenericType()
        {
            var code = "class Target<T> { }";
            var serialized = "System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(\"T:Target`1\"))";
            var type = (Type) ExecuteExpression( code, serialized )!;
            Xunit.Assert.Equal( "Target`1", type.FullName );
        }
    }
}
