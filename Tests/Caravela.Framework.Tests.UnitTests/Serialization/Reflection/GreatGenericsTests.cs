// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Serialization;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable RedundantTypeArgumentsOfMethod
#pragma warning disable SA1117 // Parameters should be on same line or separate lines

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class GreatGenericsTests : ReflectionTestBase
    {
        private readonly ObjectSerializers _serializers;
        private readonly string _code;
        private readonly IEnumerable<INamedType> _topLevelTypes;

        public GreatGenericsTests( ITestOutputHelper helper ) : base( helper )
        {
            this._serializers = new ObjectSerializers();
            this._code = @"
class Origin<T1> { 
    private T1 privateField;
    public T1 Field;
    private T1 privateProperty { get; }
    public T1 Property { get; }
    public event System.Action<T1> Actioned;
    public T1 Method(T1 parameter) => default(T1);
    public Origin(T1 input) { }
    public class NestedInOrigin<T2> : Origin<int> {
        public NestedInOrigin() : base (default) {}

        public T2 Method2(T2 input) => default(T2);
        public T2 Method21(T1 input) => default(T2);
    }
}
class Descendant<T3> : Origin<string>.NestedInOrigin<T3> {
    public T3 Field;
}
class User {
    public Descendant<float> FullyInstantiated;
}";
            var compilation = CreateCompilation( this._code );
            this._topLevelTypes = compilation.DeclaredTypes;
        }

        [Fact]
        public void TestGenericTemplates()
        {
            // Pure types
            var origin = this._topLevelTypes.Single( t => t.Name == "Origin" );
            var nested = origin.NestedTypes.Single();
            var descendant = this._topLevelTypes.Single( t => t.Name == "Descendant" );

            this.TestSerializable(
                this._code,
                nested.Method( "Method21" ),
                ( m ) =>
                {
                    Assert.Equal( "T2", m.ReturnType.Name );
                    Assert.Equal( "T1", m.GetParameters()[0].ParameterType.Name );
                },
                @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Origin`1.NestedInOrigin`1.Method21(`0)~`1""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1.NestedInOrigin`1"")).TypeHandle)" );

            this.TestSerializable( this._code, descendant.BaseType!.Method( "Method21" ), ( m ) =>
                {
                    Assert.Equal( "T3", m.ReturnType.Name );
                    Assert.Equal( "String", m.GetParameters()[0].ParameterType.Name );
                },
                @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Origin`1.NestedInOrigin`1.Method21(`0)~`1""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1.NestedInOrigin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Descendant`1"")).GetGenericArguments()[0]).TypeHandle)" );
        }

        [Fact]
        public void TestGenericInstances()
        {
            // Generic instances
            var user = this._topLevelTypes.Single( t => t.Name == "User" )!;
            var instantiatedDescendant = (INamedType) user.Fields.Single().Type;
            var instantiatedNested = instantiatedDescendant.BaseType!;
            var instantiatedBaseOrigin = instantiatedNested.BaseType!;

            this.TestSerializable( this._code, instantiatedNested.Method( "Method21" ), ( m ) =>
            {
                Assert.Equal( typeof( float ), m.ReturnType );
                Assert.Equal( typeof( string ), m.GetParameters()[0].ParameterType );
            }, @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Origin`1.NestedInOrigin`1.Method21(`0)~`1""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1.NestedInOrigin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Single""))).TypeHandle)" );
            this.TestSerializable( this._code, instantiatedNested.Constructors.Single(), c =>
            {
                Assert.Equal( typeof( string ), c.DeclaringType!.GenericTypeArguments[0] );
                Assert.Equal( typeof( float ), c.DeclaringType.GenericTypeArguments[1] );
                Assert.Equal( typeof( int ), c.DeclaringType.BaseType!.GenericTypeArguments[0] );
            }, @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Origin`1.NestedInOrigin`1.#ctor""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1.NestedInOrigin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String"")), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Single""))).TypeHandle)" );
            this.TestSerializable( this._code, ((INamedType) instantiatedNested.ContainingElement!).Method( "Method" ), m => Assert.Equal( typeof( string ), m.ReturnType ), @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Origin`1.Method(`0)~`0""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.String""))).TypeHandle)" );
            this.TestSerializable( this._code, instantiatedDescendant.Field( "Field" ), ( FieldInfo f ) => Assert.Equal( typeof( float ), f.FieldType ), @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Descendant`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Single""))).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance)" );
            this.TestSerializable( this._code, instantiatedBaseOrigin.Field( "Field" ), ( FieldInfo f ) => Assert.Equal( typeof( int ), f.FieldType ), @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))).GetField(""Field"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance)" );
            this.TestSerializable( this._code, instantiatedBaseOrigin.Field( "privateField" ), ( FieldInfo f ) => Assert.Equal( typeof( int ), f.FieldType ), @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))).GetField(""privateField"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance)" );
            this.TestSerializable( this._code, instantiatedBaseOrigin.Property( "Property" ), ( PropertyInfo p ) => Assert.Equal( typeof( int ), p.PropertyType ), @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))).GetProperty(""Property"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance)" );
            this.TestSerializable( this._code, instantiatedBaseOrigin.Property( "privateProperty" ), ( PropertyInfo p ) => Assert.Equal( typeof( int ), p.PropertyType ), @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))).GetProperty(""privateProperty"", System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance)" );
            this.TestSerializable( this._code, instantiatedBaseOrigin, t =>
            {
                Assert.Equal( "Origin`1", t.Name );
                Assert.Equal( typeof( int ), t.GenericTypeArguments[0] );
            }, @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32"")))" );
            this.TestSerializable( this._code, instantiatedBaseOrigin.Event( "Actioned" ), e =>
                {
                    Assert.Equal( "Actioned", e.Name );
                    Assert.Equal( typeof( Action<int> ), e.EventHandlerType );
                },
                @"System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Origin`1"")).MakeGenericType(System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:System.Int32""))).GetEvent(""Actioned"")" );
        }

        private void TestSerializable( string context, IType type, Action<Type> withResult, string expectedCode )
        {
            this.TestExpression<Type>( context, this._serializers.SerializeToRoslynCreationExpression( CompileTimeType.Create( type ) ).ToString(), withResult, expectedCode );
        }

        private void TestSerializable( string context, IMethod method, Action<MethodInfo> withResult, string expectedCode )
        {
            this.TestExpression<MethodInfo>( context, this._serializers.SerializeToRoslynCreationExpression( CompileTimeMethodInfo.Create( method ) ).ToString(), withResult, expectedCode );
        }

        private void TestSerializable( string context, IConstructor method, Action<ConstructorInfo> withResult, string expectedCode )
        {
            this.TestExpression<ConstructorInfo>( context, this._serializers.SerializeToRoslynCreationExpression( CompileTimeConstructorInfo.Create( method ) ).ToString(), withResult, expectedCode );
        }

        private void TestSerializable<T>( string context, IFieldOrPropertyInvocation property, Action<T> withResult, string expectedCode )
        {
            this.TestExpression<T>( context, CaravelaPropertyInfoTests.StripLocationInfo( this._serializers.SerializeToRoslynCreationExpression( CompileTimeLocationInfo.Create( property ) ).ToString() ), withResult, expectedCode );
        }

        private void TestSerializable( string context, IEvent @event, Action<EventInfo> withResult, string expectedCode )
        {
            this.TestExpression<EventInfo>( context, this._serializers.SerializeToRoslynCreationExpression( CompileTimeEventInfo.Create( @event ) ).ToString(), withResult, expectedCode );
        }

        private void TestExpression<T>( string context, string expression, Action<T> withResult, string expectedCode )
        {
            TestExpression( context, expression, withResult );
            this.AssertEqual( expectedCode, expression );
        }
    }
}