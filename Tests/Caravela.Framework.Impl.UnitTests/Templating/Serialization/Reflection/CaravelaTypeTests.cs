using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaTypeTests: ReflectionTestBase
    {  
        [Fact]
        public void TestType()
        {
            string code = "class Target {  }";
            string serialized = this.SerializeType( code );
            AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target""))", serialized );

            TestExpression<Type>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.Name );
            } );
        }
        [Fact]
        public void TestGenericType()
        {
            string code = "class Target<TKey,TValue> {  }";
            string serialized = this.SerializeType( code );
            AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`2""))", serialized );

            TestExpression<Type>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target`2", info.Name );
                Assert.Equal( 2, info.GetGenericArguments().Length );
            } );
        }
        [Fact]
        public void TestArrayType()
        {
            string code = "class Target { int[] Property { get; set; } }";
            string serialized = this.SerializeTypeOfProperty( code );
            AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32"")).MakeArrayType()", serialized );

            TestExpression<Type>( code, serialized, ( info ) =>
            {
                Assert.Equal( "System.Int32[]", info.FullName );
                Assert.Equal( typeof(int[]), info );
            } );
        }
       
        // Types other than named types and array types are not implemented.

        private string SerializeType( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IType single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" );
            string actual = new CaravelaTypeSerializer().Serialize( CaravelaType.Create( single ) ).ToString();
            return actual;
        }
        private string SerializeTypeOfProperty( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IType single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Properties.GetValue().Single( p => p.Name == "Property" ).Type;
            string actual = new CaravelaTypeSerializer().Serialize( CaravelaType.Create( single )  ).ToString();
            return actual;
        }

        public CaravelaTypeTests(ITestOutputHelper helper) : base(helper)
        {
        }
    }
}