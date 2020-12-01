using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaGenericsTests : ReflectionTestBase
    {
        private ObjectSerializers _objectSerializers;

        public CaravelaGenericsTests( ITestOutputHelper helper ) : base(helper)
        {
            this._objectSerializers = new ObjectSerializers();
        }

        [Fact]
        public void FieldInNestedType()
        {
            string code = "class Target<TKey> { class Nested<TValue> { public System.Collections.Generic.Dictionary<TKey,TValue> Field; } }";
            string serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaLocationInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single().NestedTypes.GetValue().Single().Properties.GetValue().Single() ) )
                .ToString();
            AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1.Nested`1"")).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

            TestExpression<FieldInfo>( code, CaravelaPropertyInfoTests.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Field", info.Name );
                Assert.Equal( "Dictionary`2", info.FieldType.Name );
            } );
        }
        
        [Fact]
        public void MethodInDerivedType()
        {
            string code = "class Target<TKey> : Origin<int, TKey> { TKey ReturnSelf() { return default(TKey); } } class Origin<TA, TB> { }";
            string serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single(t => t.Name == "Target").Methods.GetValue().Single() ) )
                .ToString();
            AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.ReturnSelf~`0""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).TypeHandle)", serialized );

            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "ReturnSelf", info.Name );
                Assert.Equal( "TKey", info.ReturnParameter.ParameterType.Name );
            } );
        }

        [Fact]
        public void TestHalfTypes()
        {
            string code = "class Target<T1, T2> { void Method(T1 a, T2 b) { } } class User<T> : Target<int, T> { }";
            Assert.Throws<CaravelaException>( () =>
            {
                this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single( t => t.Name == "User" ).BaseType.Methods.GetValue().Single() ) );
            } );
            string code2 = "class Target<T1, T2> { void Method(T1 a, T2 b) { } } class User<T> : Target<T, int> { }";
            Assert.Throws<CaravelaException>( () =>
            {
                this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code2 ).DeclaredTypes.GetValue().Single( t => t.Name == "User" ).BaseType.Methods.GetValue().Single() ) );
            } );
        }
    }
}