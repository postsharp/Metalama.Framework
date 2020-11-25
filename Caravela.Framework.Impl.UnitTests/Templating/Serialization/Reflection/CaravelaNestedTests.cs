using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaNestedTests : TestBase
    {
        [Fact]
        public void TestType()
        {
            string code = "class Target { class Sub { }  }";
            string serialized = this.SerializeType( code );
            Assert.Equal( @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target.Sub""))", serialized );

            TestExpression<Type>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Sub", info.Name );
            } );
        }
        private string SerializeType( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IType single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).NestedTypes.GetValue().Single( nt => nt.Name == "Sub" );
            ITypeInternal p = (single as ITypeInternal)!;
            string actual = new CaravelaTypeSerializer().Serialize( new CaravelaType( p.TypeSymbol ) ).ToString();
            return actual;
        }
    }
}