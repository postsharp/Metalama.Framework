using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaMethodInfoTests : TestBase
    {
        [Fact]
        public void TestSerializationOfMethod()
        {
            var compilation  = TestBase.CreateCompilation( "class A { public int M() => 42; }" );
            IMethod single = compilation.DeclaredTypes.GetValue().Single().Methods.GetValue().Single( m => m.Name == "M" );
            Method m = single as Method;
            string actual = new CaravelaMethodInfoSerializer().Serialize( new CaravelaMethodInfo( m.Symbol ) ).ToString();
            Assert.Equal( "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:A.M~System.Int32\"))", actual );
        }

        [Fact]
        public void TestSimpleMethod()
        {
            string code = "class Target { public static int Method(int a) => 2*a; }";
            string serialized = this.SerializeTargetDotMethod( code );
            Assert.Equal( "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method(System.Int32)~System.Int32\"))", serialized );
            var methodInfo = (MethodInfo) ExecuteExpression( code, serialized )!;
            Assert.Equal( 42, methodInfo.Invoke( null, new object[] { 21 } ) );
        }
        
        [Fact]
        public void TestGenericMethod()
        {
            string code = "class Target { public static T Method<T>(T a) => (T)(object)(2*(int)(object)a); }";
            string serialized = "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method``1(``0)~``0\"))"; //this.SerializeTargetDotMethod( code );
            Assert.Equal( "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method``1(``0)~``0\"))", serialized );
            var methodInfo = (MethodInfo) ExecuteExpression( code, serialized )!;
            Assert.Equal( 42F, methodInfo.Invoke( null, new object[] { 21F } ) );
        }

        private string SerializeTargetDotMethod( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IMethod single = compilation.DeclaredTypes.GetValue().Single(t => t.Name == "Target").Methods.GetValue().Single( m => m.Name == "Method" );
            Method m = (single as Method)!;
            string actual = new CaravelaMethodInfoSerializer().Serialize( new CaravelaMethodInfo( m.Symbol ) ).ToString();
            return actual;
        }

        class A
        {
            public int M() => 42;
        }
        
        [Fact]
        public void TestNewMethodInfo()
        {
            var methodInfo = System.Reflection.MethodBase.GetMethodFromHandle( typeof(A).GetMethod( "M" ).MethodHandle );
            Assert.Equal( 42, methodInfo.Invoke( new A(), null ) );
        }
    }
}