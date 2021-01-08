using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaConstructorInfoTests : ReflectionTestBase
    {  
        [Fact]
        public void TestConstructor()
        {
            string code = "class Target { public Target(int hello) { } }";
            string serialized = this.SerializeConstructor( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.#ctor(System.Int32)""))", serialized );

            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.DeclaringType!.Name );
                Assert.Single( info.GetParameters());
            } );
        }
        
        [Fact]
        public void TestGenericConstructor()
        {
            string code = "class Target<T> where T: struct { public Target(T hello) { } }";
            string serialized = this.SerializeConstructor( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.#ctor(`0)""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).TypeHandle)", serialized );
           
            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target`1", info.DeclaringType!.Name );
                Assert.Single( info.GetParameters());
            } );
        }
        
        [Fact]
        public void TestDefaultConstructor()
        {
            string code = "class Target {  }";
            string serialized = this.SerializeConstructor( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.#ctor""))", serialized );

            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.DeclaringType!.Name );
                Assert.Empty( info.GetParameters());
            } );
        }
        
        // If there is no constructor, there is no constructor to serialize. We are at C#, not IL level.
        private string SerializeConstructor( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            var namedTypes = compilation.DeclaredTypes.GetValue();
            INamedType type = namedTypes.Single( t => t.Name == "Target" );
            IEnumerable<IMethod> methods = type.AllMethods.GetValue();
            IMethod single = methods.Single( m => m.Name == ".ctor" );
            Method p = (single as Method)!;
            string actual = new CaravelaConstructorInfoSerializer(new CaravelaTypeSerializer()).Serialize( new CaravelaConstructorInfo( p ) ).ToString();
            return actual;
        }

        public CaravelaConstructorInfoTests(ITestOutputHelper helper) : base(helper)
        {
        }
    }
}