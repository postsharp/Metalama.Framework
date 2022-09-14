// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.ReflectionMocks;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace Metalama.Framework.Tests.UnitTests.SyntaxSerialization.Reflection
{
    public class MetalamaParameterInfoTests : ReflectionTestBase
    {
        public MetalamaParameterInfoTests( ITestOutputHelper helper ) : base( helper ) { }

        [Fact]
        public void TestParameter()
        {
            var code = "class Target { public static int Method(int target) => 2*target; }";
            var serialized = this.SerializeParameter( code );

            this.AssertEqual(
                @"global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Method(int)"").GetParameters()[0]",
                serialized );

            this.TestExpression<ParameterInfo>(
                code,
                serialized,
                parameterInfo =>
                {
                    Assert.Equal( "target", parameterInfo.Name );
                    Assert.Equal( 0, parameterInfo.Position );
                    Assert.Equal( typeof(int), parameterInfo.ParameterType );
                } );
        }

        [Fact]
        public void TestGenericParameter_GenericInMethod()
        {
            var code = "class Target { public static int Method<T>(T target) => 4; }";
            var serialized = this.SerializeParameter( code );

            this.AssertEqual(
                @"global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Method<T>(T)"").GetParameters()[0]",
                serialized );

            this.TestExpression<ParameterInfo>(
                code,
                serialized,
                parameterInfo =>
                {
                    Assert.Equal( "target", parameterInfo.Name );
                    Assert.Equal( 0, parameterInfo.Position );
                    Assert.Equal( "T", parameterInfo.ParameterType.Name );
                } );
        }

        [Fact]
        public void TestGenericParameter_GenericInTypeAndMethod()
        {
            var code = "class Target<T> { public static int Method<U>(System.Tuple<T,U> target) => 4; }";
            var serialized = this.SerializeParameter( code );

            this.AssertEqual(
                @"global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target<>), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target<T>.Method<U>(Tuple<T, U>)"").GetParameters()[0]",
                serialized );

            this.TestExpression<ParameterInfo>(
                code,
                serialized,
                parameterInfo =>
                {
                    Assert.Equal( "target", parameterInfo.Name );
                    Assert.Equal( 0, parameterInfo.Position );
                    Assert.Equal( "Tuple`2", parameterInfo.ParameterType.Name );
                } );
        }

        [Fact]
        public void TestParameterInSecondPlace()
        {
            var code = "class Target { public static int Method(float ignored, int target) => 2*target; }";
            var serialized = this.SerializeParameter( code );

            this.AssertEqual(
                @"global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Method(float, int)"").GetParameters()[1]",
                serialized );

            this.TestExpression<ParameterInfo>(
                code,
                serialized,
                parameterInfo =>
                {
                    Assert.Equal( "target", parameterInfo.Name );
                    Assert.Equal( 1, parameterInfo.Position );
                    Assert.Equal( typeof(int), parameterInfo.ParameterType );
                } );
        }

        [Fact]
        public void TestReturnParameter()
        {
            var code = "class Target { public static string Method(float ignored, int target) => null; }";
            var serialized = this.SerializeReturnParameter( code );

            this.AssertEqual(
                @"((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""Method"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Method(float, int)"")).ReturnParameter",
                serialized );

            this.TestExpression<ParameterInfo>(
                code,
                serialized,
                parameterInfo =>
                {
                    Assert.Equal( -1, parameterInfo.Position );
                    Assert.Equal( typeof(string), parameterInfo.ParameterType );
                } );
        }

        [Fact]
        public void TestReturnParameterOfProperty()
        {
            var code = "class Target { public static string Property => null; }";
            var serialized = this.SerializeReturnParameterOfProperty( code );

            this.AssertEqual(
                @"((global::System.Reflection.MethodInfo)global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""get_Property"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Static, ""Target.Property.get"")).ReturnParameter",
                serialized );

            this.TestExpression<ParameterInfo>(
                code,
                serialized,
                parameterInfo =>
                {
                    Assert.Equal( -1, parameterInfo.Position );
                    Assert.Equal( typeof(string), parameterInfo.ParameterType );
                } );
        }

        [Fact]
        public void TestParameterOfIndexer()
        {
            var code = "class Target { public int this[int target] { get {return 0;} set{} }}";
            var serialized = this.SerializeIndexerParameter( code );

            this.AssertEqual(
                @"global::Metalama.Framework.RunTime.ReflectionHelper.GetMethod(typeof(global::Target), ""get_Item"", global::System.Reflection.BindingFlags.Public | global::System.Reflection.BindingFlags.Instance, ""Target.this[int].get"").GetParameters()[0]",
                serialized );

            this.TestExpression<ParameterInfo>(
                code,
                serialized,
                parameterInfo =>
                {
                    Assert.Equal( "target", parameterInfo.Name );
                    Assert.Equal( 0, parameterInfo.Position );
                    Assert.Equal( typeof(int), parameterInfo.ParameterType );
                } );
        }

        private string SerializeIndexerParameter( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var targetType = compilation.Types.Single( t => t.Name == "Target" );
            var single = targetType.Indexers.Single().Parameters.First( p => p.Name == "target" );
            var parameter = single;

            var actual =
                testContext.Serialize( CompileTimeParameterInfo.Create( parameter ) )
                    .ToString();

            return actual;
        }

        private string SerializeParameter( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;

            var single = compilation.Types.Single( t => t.Name == "Target" )
                .Methods.Single( m => m.Name == "Method" )
                .Parameters.First( p => p.Name == "target" );

            var parameter = (Parameter) single;

            var actual =
                testContext.Serialize( CompileTimeParameterInfo.Create( parameter ) )
                    .ToString();

            return actual;
        }

        private string SerializeReturnParameter( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var single = compilation.Types.Single( t => t.Name == "Target" ).Methods.Single( m => m.Name == "Method" ).ReturnParameter;
            var p = (MethodReturnParameter) single;

            var actual = testContext.Serialize( CompileTimeReturnParameterInfo.Create( p ) )
                .ToString();

            return actual;
        }

        private string SerializeReturnParameterOfProperty( string code )
        {
            using var testContext = this.CreateSerializationTestContext( code );

            var compilation = testContext.Compilation;
            var single = compilation.Types.Single( t => t.Name == "Target" ).Properties.Single( m => m.Name == "Property" ).GetMethod!.ReturnParameter;
            var p = (MethodReturnParameter) single;

            var actual = testContext.Serialize( CompileTimeReturnParameterInfo.Create( p ) )
                .ToString();

            return actual;
        }
    }
}