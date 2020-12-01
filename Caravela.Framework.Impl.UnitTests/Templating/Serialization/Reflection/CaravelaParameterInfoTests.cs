using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaParameterInfoTests : ReflectionTestBase
    {
        private CaravelaMethodInfoSerializer _caravelaMethodInfoSerializer;

        public CaravelaParameterInfoTests( ITestOutputHelper helper ) : base(helper)
        {
            _caravelaMethodInfoSerializer = new CaravelaMethodInfoSerializer(new CaravelaTypeSerializer());
        }

        [Fact]
        public void TestParameter()
        {
            string code = "class Target { public static int Method(int target) => 2*target; }";
            string serialized = this.SerializeParameter( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Int32)~System.Int32"")).GetParameters()[0]", serialized );
            
            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( "target", parameterInfo.Name );
                Assert.Equal( 0, parameterInfo.Position );
                Assert.Equal( typeof(int), parameterInfo.ParameterType );
            } );
        }
        
        [Fact]
        public void TestGenericParameter_GenericInMethod()
        {
            string code = "class Target { public static int Method<T>(T target) => 4; }";
            string serialized = this.SerializeParameter( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method``1(``0)~System.Int32"")).GetParameters()[0]", serialized );
            
            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( "target", parameterInfo.Name );
                Assert.Equal( 0, parameterInfo.Position );
                Assert.Equal( "T", parameterInfo.ParameterType.Name );
            } );
        }
        [Fact]
        public void TestGenericParameter_GenericInTypeAndMethod()
        {
            string code = "class Target<T> { public static int Method<U>(System.Tuple<T,U> target) => 4; }";
            string serialized = this.SerializeParameter( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.Method``1(System.Tuple{`0,``0})~System.Int32""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).TypeHandle).GetParameters()[0]", serialized );
            
            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( "target", parameterInfo.Name );
                Assert.Equal( 0, parameterInfo.Position );
                Assert.Equal( "Tuple`2", parameterInfo.ParameterType.Name );
            } );
        }

        [Fact]
        public void TestParameterInSecondPlace()
        {
            string code = "class Target { public static int Method(float ignored, int target) => 2*target; }";
            string serialized = this.SerializeParameter( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Single,System.Int32)~System.Int32"")).GetParameters()[1]", serialized );

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
            AssertEqual( @"((System.Reflection.MethodInfo)System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Single,System.Int32)~System.String""))).ReturnParameter", serialized );

            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( -1, parameterInfo.Position );
                Assert.Equal( typeof(string), parameterInfo.ParameterType );
            } );
        }
        
        [Fact]
        public void TestReturnParameterOfProperty()
        {
            string code = "class Target { public static string Property => null; }";
            string serialized = this.SerializeReturnParameterOfProperty( code );
            AssertEqual( @"((System.Reflection.MethodInfo)System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.get_Property~System.String""))).ReturnParameter", serialized );

            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( -1, parameterInfo.Position );
                Assert.Equal( typeof(string), parameterInfo.ParameterType );
            } );
        }
        
        [Fact]
        public void TestParameterOfIndexer()
        {
            string code = "class Target { public int this[int target] { get {return 0;} set{} }}";
            string serialized = this.SerializeIndexerParameter( code );
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.set_Item(System.Int32,System.Int32)"")).GetParameters()[0]", serialized );
            
            TestExpression<ParameterInfo>( code, serialized, ( parameterInfo ) =>
            {
                Assert.Equal( "target", parameterInfo.Name );
                Assert.Equal( 0, parameterInfo.Position );
                Assert.Equal( typeof(int), parameterInfo.ParameterType );
            } );
        }
        
        private string SerializeIndexerParameter( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            INamedType targetType = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" );
            IParameter single = targetType.Properties.GetValue().Single( m => m.Name == "this[]" ).Parameters.First(p => p.Name=="target");
            Parameter p = (single as Parameter)!;
            string actual = new CaravelaParameterInfoSerializer(this._caravelaMethodInfoSerializer).Serialize( new CaravelaParameterInfo( p.Symbol, p.ContainingElement ) ).ToString();
            return actual;
        }
        private string SerializeParameter( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IParameter single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Methods.GetValue().Single( m => m.Name == "Method" ).Parameters.First(p => p.Name=="target");
            Parameter p = (single as Parameter)!;
            string actual = new CaravelaParameterInfoSerializer(this._caravelaMethodInfoSerializer).Serialize( new CaravelaParameterInfo( p.Symbol, p.ContainingElement ) ).ToString();
            return actual;
        }
        private string SerializeReturnParameter( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IParameter single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Methods.GetValue().Single( m => m.Name == "Method" ).ReturnParameter;
            Method.ReturnParameterImpl p = (single as Method.ReturnParameterImpl)!;
            string actual = new CaravelaReturnParameterInfoSerializer(this._caravelaMethodInfoSerializer).Serialize( new CaravelaReturnParameterInfo( p ) ).ToString();
            return actual;
        }
        private string SerializeReturnParameterOfProperty( string code )
        {
            var compilation  = TestBase.CreateCompilation( code );
            IParameter single = compilation.DeclaredTypes.GetValue().Single( t => t.Name == "Target" ).Properties.GetValue().Single( m => m.Name == "Property" ).Getter.ReturnParameter;
            Method.ReturnParameterImpl p = (single as Method.ReturnParameterImpl)!;
            string actual = new CaravelaReturnParameterInfoSerializer(this._caravelaMethodInfoSerializer).Serialize( new CaravelaReturnParameterInfo( p ) ).ToString();
            return actual;
        }
    }
}