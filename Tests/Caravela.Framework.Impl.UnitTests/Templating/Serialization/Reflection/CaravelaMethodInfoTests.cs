using System.Linq;
using System.Reflection;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaMethodInfoTests : TestBase
    {
        [Fact]
        public void TestSerializationOfMethod()
        {
            var code = "class Target { public static int Method() => 42; }";
            var serialized = this.SerializeTargetDotMethod( code );
            Assert.Equal( "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method~System.Int32\"))", serialized );

            TestExpression<MethodInfo>( code, serialized, ( info ) =>
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
            Assert.Equal( "System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(\"M:Target.Method``1(``0)~``0\"))", serialized );

            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.DeclaringType!.Name );
                Assert.Equal( "Method", info.Name );
                Assert.Equal( 42, info.MakeGenericMethod( new[] { typeof( int ) } ).Invoke( null, new object[] { 21 } ) );
            } );
        }

        private string SerializeTargetDotMethod( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Methods.GetValue().Single( m => m.Name == "Method" );
            var method = (Method) single;
            var actual = new CaravelaMethodInfoSerializer( new CaravelaTypeSerializer() ).Serialize( new CaravelaMethodInfo( method ) ).ToString();
            return actual;
        }
    }
}