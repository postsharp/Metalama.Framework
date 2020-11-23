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
    public class CaravelaMethodInfoTests
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