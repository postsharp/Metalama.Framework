using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaNestedTests : ReflectionTestBase
    {
        [Fact]
        public void TestType()
        {
            var code = "class Target { class Sub { }  }";
            var serialized = this.SerializeType( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target.Sub""))", serialized );

            TestExpression<Type>( code, serialized, ( info ) => Assert.Equal( "Sub", info.Name ) );
        }

        private string SerializeType( string code )
        {
            var compilation = CreateCompilation( code );
            IType single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).NestedTypes.GetValue().Single( nt => nt.Name == "Sub" );
            return new CaravelaTypeSerializer().Serialize( CaravelaType.Create( single ) ).ToString();
        }

        public CaravelaNestedTests( ITestOutputHelper helper ) : base( helper )
        {
        }
    }
}