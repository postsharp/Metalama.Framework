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
    }
}
