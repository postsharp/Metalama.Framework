using System;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Impl.Templating.Serialization;
using Caravela.Framework.Impl.Templating.Serialization.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Impl.UnitTests.Templating.Serialization.Reflection
{
    public class CaravelaGenericsTests : ReflectionTestBase
    {
        private readonly ObjectSerializers _objectSerializers;

        public CaravelaGenericsTests( ITestOutputHelper helper ) : base( helper )
        {
            this._objectSerializers = new ObjectSerializers();
        }

        [Fact]
        public void FieldInNestedType()
        {
            var code = "class Target<TKey> { class Nested<TValue> { public System.Collections.Generic.Dictionary<TKey,TValue> Field; } }";
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaLocationInfo.Create( CreateCompilation( code ).DeclaredTypes.Single().NestedTypes.Single().Properties.Single() ) )
                .ToString();
            this.AssertEqual( @"new Caravela.Framework.LocationInfo(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1.Nested`1"")).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance))", serialized );

            TestExpression<FieldInfo>( code, CaravelaPropertyInfoTests.StripLocationInfo( serialized ), ( info ) =>
            {
                Assert.Equal( "Field", info.Name );
                Assert.Equal( "Dictionary`2", info.FieldType.Name );
            } );
        }

        [Fact]
        public void MethodInDerivedType()
        {
            var code = "class Target<TKey> : Origin<int, TKey> { TKey ReturnSelf() { return default(TKey); } } class Origin<TA, TB> { }";
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.Single( t => t.Name == "Target" ).Methods.First() ) )
                .ToString();
            this.AssertEqual( @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.ReturnSelf~`0""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).TypeHandle)", serialized );

            TestExpression<MethodInfo>( code, serialized, ( info ) =>
            {
                Assert.Equal( "ReturnSelf", info.Name );
                Assert.Equal( "TKey", info.ReturnParameter.ParameterType.Name );
            } );
        }

        [Fact]
        public void TestDerivedGenericType()
        {
            var code = "class Target<T1> { } class User<T2> : Target<T2> { }";
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaType.Create( CreateCompilation( code ).DeclaredTypes.Single( t => t.Name == "User" ).BaseType! ) )
                .ToString();
            TestExpression<Type>( code, serialized, ( info ) => Assert.Equal( "Target`1[T2]", info.ToString() ) );
            var serialized2 = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaType.Create( CreateCompilation( code ).DeclaredTypes.Single( t => t.Name == "Target" ) ) )
                .ToString();
            TestExpression<Type>( code, serialized2, ( info ) => Assert.Equal( "Target`1[T1]", info.ToString() ) );
        }

        [Fact]
        public void TestHalfTypes()
        {
            var code = "class Target<T1, T2> { void Method(T1 a, T2 b) { } } class User<T> : Target<int, T> { }";
            var serialized = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code ).DeclaredTypes.Single( t => t.Name == "User" ).BaseType!.Methods.First() ) )
                .ToString();
            TestExpression<MethodInfo>( code, serialized, ( info ) => Assert.Equal( "Target`2[System.Int32,T]", info.DeclaringType?.ToString() ) );
            var code2 = "class Target<T1, T2> { void Method(T1 a, T2 b) { } } class User<T> : Target<T, int> { }";
            var serialized2 = this._objectSerializers.SerializeToRoslynCreationExpression( CaravelaMethodInfo.Create( CreateCompilation( code2 ).DeclaredTypes.Single( t => t.Name == "User" ).BaseType!.Methods.First() ) )
                .ToString();
            TestExpression<MethodInfo>( code2, serialized2, ( info ) => Assert.Equal( "Target`2[T,System.Int32]", info.DeclaringType?.ToString() ) );
        }
    }
}