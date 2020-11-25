using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaConstructorInfoTests : TestBase
    {  
        [Fact]
        public void TestConstructor()
        {
            string code = "class Target { public Target(int hello) { } }";
            string serialized = this.SerializeConstructor( code );
            Assert.Equal( @"xxxx", serialized );

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
            Assert.Equal( @"xxxx", serialized );

            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target`1", info.DeclaringType!.Name );
                Assert.Single( info.GetParameters());
            } );
        }
        
        [Fact]
        public void TestNoConstructor()
        {
            string code = "class Target { }";
            string serialized = this.SerializeConstructor( code );
            Assert.Equal( @"xxxx", serialized );

            TestExpression<ConstructorInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "Target", info.DeclaringType!.Name );
                Assert.Empty( info.GetParameters());
            } );
        }

        private string SerializeConstructor( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IMethod single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Methods.GetValue().Single( m => m.Name == ".ctor" );
            Method p = (single as Method)!;
            string actual = new CaravelaConstructorInfoSerializer().Serialize( new CaravelaConstructorInfo( p.Symbol ) ).ToString();
            return actual;
        }
    }
}