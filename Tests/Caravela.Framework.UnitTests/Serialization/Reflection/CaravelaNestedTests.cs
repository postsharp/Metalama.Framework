using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaNestedTests : ReflectionTestBase
    {
        [Fact]
        public void TestType()
        {
            var code = "class Target { class Sub { }  }";
            var serialized = this.SerializeType( code );
            this.AssertEqual( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target.Sub""))", serialized );

            TestExpression( code, serialized, ( Type info ) => Xunit.Assert.Equal( "Sub", info.Name ) );
        }

        private string SerializeType( string code )
        {
            var compilation = CreateCompilation( code );
            IType single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).NestedTypes.Single( nt => nt.Name == "Sub" );
            return new CaravelaTypeSerializer().Serialize( CaravelaType.Create( single ) ).ToString();
        }

        public CaravelaNestedTests( ITestOutputHelper helper ) : base( helper )
        {
        }
    }
}