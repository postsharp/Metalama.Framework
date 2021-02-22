using System.Linq;
using System.Reflection;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Caravela.Framework.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaConstructorInfoTests : ReflectionTestBase
    {
        [Fact]
        public void TestConstructor()
        {
            var code = "class Target { public Target(int hello) { } }";
            var serialized = this.SerializeConstructor( code );
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.#ctor(System.Int32)""))", serialized );

            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
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
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.#ctor(`0)""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).TypeHandle)", serialized );

            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
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
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.#ctor""))", serialized );

            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.DeclaringType!.Name );
                Assert.Empty( info.GetParameters() );
            } );
        }

        // If there is no constructor, there is no constructor to serialize. We are at C#, not IL level.
        private string SerializeConstructor( string code )
        {
            var compilation = CreateCompilation( code );
            var namedTypes = compilation.DeclaredTypes;
            var type = namedTypes.Single( t => t.Name == "Target" );
            var methods = type.Constructors;
            var single = methods.Single();
            var p = (single as Constructor)!;
            var actual = new CaravelaConstructorInfoSerializer( new CaravelaTypeSerializer() ).Serialize( new CaravelaConstructorInfo( p ) ).ToString();
            return actual;
        }

        public CaravelaConstructorInfoTests( ITestOutputHelper helper ) : base( helper )
        {
        }
    }
}