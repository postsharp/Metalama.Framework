using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaParameterInfoTests : TestBase
    {
        [Fact]
        public void TestParameter()
        {
            string code = "class Target { public static int Method(int target) => 2*target; }";
            string serialized = this.SerializeParameter( code );
            Assert.Equal( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Int32)~System.Int32"")).GetParameters()[0]", serialized );
            
            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( "target", parameterInfo.Name );
                Assert.Equal( 0, parameterInfo.Position );
                Assert.Equal( typeof(int), parameterInfo.ParameterType );
            } );
        }
        
        [Fact]
        public void TestParameterInSecondPlace()
        {
            string code = "class Target { public static int Method(float ignored, int target) => 2*target; }";
            string serialized = this.SerializeParameter( code );
            Assert.Equal( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Single,System.Int32)~System.Int32"")).GetParameters()[1]", serialized );

            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( "target", parameterInfo.Name );
                Assert.Equal( 1, parameterInfo.Position );
                Assert.Equal( typeof(int), parameterInfo.ParameterType );
            } );
        }
        
        [Fact]
        public void TestReturnParameter()
        {
            string code = "class Target { public static string Method(float ignored, int target) => null; }";
            string serialized = this.SerializeReturnParameter( code );
            Assert.Equal( @"((System.Reflection.MethodInfo)System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Single,System.Int32)~System.String""))).ReturnParameter", serialized );

            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( -1, parameterInfo.Position );
                Assert.Equal( typeof(string), parameterInfo.ParameterType );
            } );
        }

        private int this[ int target ]
        {
            set {}
            get { return 0; }
        }
        [Fact]
        public void TestParameterOfIndexer()
        {
            string code = "class Target { public int this[int target] { get {return 0;} set{} }}";
            string serialized = this.SerializeIndexerParameter( code );
            Assert.Equal( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.set_Item(System.Int32,System.Int32)"")).GetParameters()[0]", serialized );
            
            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( "target", parameterInfo.Name );
                Assert.Equal( 0, parameterInfo.Position );
                Assert.Equal( typeof(int), parameterInfo.ParameterType );
            } );
        }
        
        // TODO test generics
        private string SerializeIndexerParameter( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            INamedType targetType = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" );
            IParameter single = targetType.Properties.GetValue().Single( m => m.Name == "this[]" ).Parameters.First(p => p.Name=="target");
            Parameter p = (single as Parameter)!;
            string actual = new CaravelaParameterInfoSerializer(new CaravelaMethodInfoSerializer()).Serialize( new CaravelaParameterInfo( p.Symbol, p.ContainingElement ) ).ToString();
            return actual;
        }
        private string SerializePropertyReturnParameter( string code, string propertyName )
        {
            // TODO return parameters of properties
            var compilation  = TestBase.CreateCompilation( code );
            INamedType targetType = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" );
            IParameter single = targetType.Properties.GetValue().Single( m => m.Name == propertyName ).Getter.ReturnParameter;
            Parameter p = (single as Parameter)!;
            string actual = new CaravelaParameterInfoSerializer(new CaravelaMethodInfoSerializer()).Serialize( new CaravelaParameterInfo( p.Symbol, p.ContainingElement ) ).ToString();
            return actual;
        }
        private string SerializeParameter( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IParameter single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Methods.GetValue().Single( m => m.Name == "Method" ).Parameters.First(p => p.Name=="target");
            Parameter p = (single as Parameter)!;
            string actual = new CaravelaParameterInfoSerializer(new CaravelaMethodInfoSerializer()).Serialize( new CaravelaParameterInfo( p.Symbol, p.ContainingElement ) ).ToString();
            return actual;
        }
        private string SerializeReturnParameter( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IParameter single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Methods.GetValue().Single( m => m.Name == "Method" ).ReturnParameter;
            Method.ReturnParameterImpl p = (single as Method.ReturnParameterImpl)!;
            string actual = new CaravelaReturnParameterInfoSerializer(new CaravelaMethodInfoSerializer()).Serialize( new CaravelaReturnParameterInfo( p ) ).ToString();
            return actual;
        }
    }
}