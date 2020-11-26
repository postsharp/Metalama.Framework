using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaGenericsTests : TestBase
    {
        private ObjectSerializers _objectSerializers;

        public CaravelaGenericsTests()
        {
            this._objectSerializers = new ObjectSerializers();
        }

        [Fact]
        public void FieldInNestedType()
        {
            string code = "class Target<TKey> { class Nested<TValue> { public System.Collections.Generic.Dictionary<TKey,TValue> Field; } }";
            string serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaLocationInfo.Create( CreateCompilation( code ).DeclaredTypes.GetValue().Single().NestedTypes.GetValue().Single().Properties.GetValue().Single() ) )
                .ToString();
            Assert.Equal( @"new Caravela.Framework.LocationInfo(System.Reflection.FieldInfo.GetFieldFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeFieldHandle(""F:Target`1.Nested`1.Field""), Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1.Nested`1"")))", serialized );

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
            Assert.Equal( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.ReturnSelf~`0""), Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1""))", serialized );

            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "ReturnSelf", info.Name );
                Assert.Equal( "TKey", info.ReturnParameter.ParameterType.Name );
            } );
        }
    }
}