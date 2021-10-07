// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.ReflectionMocks;
using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaGenericsTests : ReflectionTestBase
    {
        public CaravelaGenericsTests( ITestOutputHelper helper ) : base( helper ) { }

        [Fact]
        public void FieldInNestedType()
        {
            using var testContext = this.CreateTestContext();

            var code = "class Target<TKey> { class Nested<TValue> { public System.Collections.Generic.Dictionary<TKey,TValue> Field; } }";

            var serialized = testContext.Serialize(
                    CompileTimeFieldOrPropertyInfo.Create( testContext.CreateCompilationModel( code ).Types.Single().NestedTypes.Single().Fields.Single() ) )
                .ToString();

            this.AssertEqual(
                @"new global::Caravela.Framework.RunTime.FieldOrPropertyInfo(typeof(global::Target<>.Nested<>).GetField(""Field"", global::System.Reflection.BindingFlags.DeclaredOnly | global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Static | global::System.Reflection.BindingFlags.Instance))",
                serialized );

            TestExpression<FieldInfo>(
                code,
                CaravelaPropertyInfoTests.StripLocationInfo( serialized ),
                info =>
                {
                    Assert.Equal( "Field", info.Name );
                    Assert.Equal( "Dictionary`2", info.FieldType.Name );
                } );
        }

        [Fact]
        public void MethodInDerivedType()
        {
            using var testContext = this.CreateTestContext();

            var code = "class Target<TKey> : Origin<int, TKey> { TKey ReturnSelf() { return default(TKey); } } class Origin<TA, TB> { }";

            var serialized = testContext.Serialize(
                    CompileTimeMethodInfo.Create( testContext.CreateCompilationModel( code ).Types.Single( t => t.Name == "Target" ).Methods.First() ) )
                .ToString();

            this.AssertEqual(
                @"((global::System.Reflection.MethodInfo)global::System.Reflection.MethodBase.GetMethodFromHandle(global::Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.ReturnSelf~`0""), typeof(global::Target<>).TypeHandle))",
                serialized );

            TestExpression<MethodInfo>(
                code,
                serialized,
                info =>
                {
                    Assert.Equal( "ReturnSelf", info.Name );
                    Assert.Equal( "TKey", info.ReturnParameter.ParameterType.Name );
                } );
        }

        [Fact]
        public void TestDerivedGenericType()
        {
            using var testContext = this.CreateTestContext();

            var code = "class Target<T1> { } class User<T2> : Target<T2> { }";

            var serialized = testContext
                .Serialize( CompileTimeType.Create( testContext.CreateCompilationModel( code ).Types.Single( t => t.Name == "User" ).BaseType! ) )
                .ToString();

            TestExpression<Type>( code, serialized, info => Assert.Equal( "Target`1[T2]", info.ToString() ) );

            var serialized2 = testContext
                .Serialize( CompileTimeType.Create( testContext.CreateCompilationModel( code ).Types.Single( t => t.Name == "Target" ) ) )
                .ToString();

            TestExpression<Type>( code, serialized2, info => Assert.Equal( "Target`1[T1]", info.ToString() ) );
        }

        [Fact]
        public void TestHalfTypes()
        {
            using var testContext = this.CreateTestContext();

            var code = "class Target<T1, T2> { void Method(T1 a, T2 b) { } } class User<T> : Target<int, T> { }";

            var serialized = testContext.Serialize(
                    CompileTimeMethodInfo.Create( testContext.CreateCompilationModel( code ).Types.Single( t => t.Name == "User" ).BaseType!.Methods.First() ) )
                .ToString();

            TestExpression<MethodInfo>( code, serialized, info => Assert.Equal( "Target`2[System.Int32,T]", info.DeclaringType?.ToString() ) );
            var code2 = "class Target<T1, T2> { void Method(T1 a, T2 b) { } } class User<T> : Target<T, int> { }";

            var serialized2 = testContext.Serialize(
                    CompileTimeMethodInfo.Create(
                        testContext.CreateCompilationModel( code2 ).Types.Single( t => t.Name == "User" ).BaseType!.Methods.First() ) )
                .ToString();

            TestExpression<MethodInfo>( code2, serialized2, info => Assert.Equal( "Target`2[T,System.Int32]", info.DeclaringType?.ToString() ) );
        }
    }
}