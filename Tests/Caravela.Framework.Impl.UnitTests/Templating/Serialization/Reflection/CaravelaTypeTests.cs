using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaTypeTests : ReflectionTestBase
    {
        [Fact]
        public void TestType()
        {
            var code = "class Target {  }";
            var serialized = this.SerializeType( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target""))", serialized );

            TestExpression<Type>( code, serialized, ( info ) => Assert.Equal( "Target", info.Name ) );
        }

        [Fact]
        public void TestGenericType()
        {
            var code = "class Target<TKey,TValue> {  }";
            var serialized = this.SerializeType( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`2""))", serialized );

            TestExpression<Type>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target`2", info.Name );
                Assert.Equal( 2, info.GetGenericArguments().Length );
            } );
        }

        [Fact]
        public void TestArrayType()
        {
            var code = "class Target { int[] Property { get; set; } }";
            var serialized = this.SerializeTypeOfProperty( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32"")).MakeArrayType()", serialized );

            TestExpression<Type>( code, serialized, ( info ) =>
            {
                Assert.Equal( "System.Int32[]", info.FullName );
                Assert.Equal( typeof( int[] ), info );
            } );
        }

        [Fact]
        public void TestMultidimensionalArrayType()
        {
            var code = "class Target { int[,] Property { get; set; } }";
            var serialized = this.SerializeTypeOfProperty( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32"")).MakeArrayType(2)", serialized );

            TestExpression<Type>( code, serialized, ( info ) =>
            {
                Assert.Equal( "System.Int32[,]", info.FullName );
                Assert.Equal( typeof( int[,] ), info );
            } );
        }

        // Types other than named types and array types are not implemented.

        private string SerializeType( string code )
        {
            var compilation = CreateCompilation( code );
            IType single = compilation.DeclaredTypes.Single( t => t.Name == "Target" );
            var actual = new CaravelaTypeSerializer().Serialize( CaravelaType.Create( single ) ).ToString();
            return actual;
        }

        private string SerializeTypeOfProperty( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Properties.Single( p => p.Name == "Property" ).Type;
            var actual = new CaravelaTypeSerializer().Serialize( CaravelaType.Create( single ) ).ToString();
            return actual;
        }

        public CaravelaTypeTests( ITestOutputHelper helper ) : base( helper )
        {
        }
    }
}