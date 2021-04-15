// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Serialization;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.UnitTests.Serialization.Reflection
{
    public class CaravelaParameterInfoTests : ReflectionTestBase
    {
        private readonly CaravelaMethodInfoSerializer _caravelaMethodInfoSerializer;

        public CaravelaParameterInfoTests( ITestOutputHelper helper ) : base( helper )
        {
            this._caravelaMethodInfoSerializer = new CaravelaMethodInfoSerializer( new CaravelaTypeSerializer() );
        }

        [Fact]
        public void TestParameter()
        {
            var code = "class Target { public static int Method(int target) => 2*target; }";
            var serialized = this.SerializeParameter( code );

            this.AssertEqual(
                @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Int32)~System.Int32"")).GetParameters()[0]",
                serialized );

            TestExpression<ParameterInfo>(
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
                @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method``1(``0)~System.Int32"")).GetParameters()[0]",
                serialized );

            TestExpression<ParameterInfo>(
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
                @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target`1.Method``1(System.Tuple{`0,``0})~System.Int32""), System.Type.GetTypeFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeTypeHandle(""T:Target`1"")).TypeHandle).GetParameters()[0]",
                serialized );

            TestExpression<ParameterInfo>(
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
                @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Single,System.Int32)~System.Int32"")).GetParameters()[1]",
                serialized );

            TestExpression<ParameterInfo>(
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
                @"((System.Reflection.MethodInfo)System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.Method(System.Single,System.Int32)~System.String""))).ReturnParameter",
                serialized );

            TestExpression<ParameterInfo>(
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
                @"((System.Reflection.MethodInfo)System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.get_Property~System.String""))).ReturnParameter",
                serialized );

            TestExpression<ParameterInfo>(
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
                @"System.Reflection.MethodBase.GetMethodFromHandle(Caravela.Compiler.Intrinsics.GetRuntimeMethodHandle(""M:Target.set_Item(System.Int32,System.Int32)"")).GetParameters()[0]",
                serialized );

            TestExpression<ParameterInfo>(
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
            var compilation = CreateCompilation( code );
            var targetType = compilation.DeclaredTypes.Single( t => t.Name == "Target" );
            var single = targetType.Properties.Single( m => m.Name == "this[]" ).Parameters.First( p => p.Name == "target" );
            var parameter = (Parameter) single;

            var actual = new CaravelaParameterInfoSerializer( this._caravelaMethodInfoSerializer )
                         .Serialize( new CompileTimeParameterInfo( parameter.ParameterSymbol, parameter.ContainingElement ) )
                         .ToString();

            return actual;
        }

        private string SerializeParameter( string code )
        {
            var compilation = CreateCompilation( code );

            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" )
                                    .Methods.Single( m => m.Name == "Method" )
                                    .Parameters.First( p => p.Name == "target" );

            var parameter = (Parameter) single;

            var actual = new CaravelaParameterInfoSerializer( this._caravelaMethodInfoSerializer )
                         .Serialize( new CompileTimeParameterInfo( parameter.ParameterSymbol, parameter.ContainingElement ) )
                         .ToString();

            return actual;
        }

        private string SerializeReturnParameter( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Methods.Single( m => m.Name == "Method" ).ReturnParameter!;
            var p = (MethodReturnParameter) single;

            var actual = new CaravelaReturnParameterInfoSerializer( this._caravelaMethodInfoSerializer ).Serialize( new CompileTimeReturnParameterInfo( p ) )
                                                                                                        .ToString();

            return actual;
        }

        private string SerializeReturnParameterOfProperty( string code )
        {
            var compilation = CreateCompilation( code );
            var single = compilation.DeclaredTypes.Single( t => t.Name == "Target" ).Properties.Single( m => m.Name == "Property" ).Getter!.ReturnParameter!;
            var p = (MethodReturnParameter) single;

            var actual = new CaravelaReturnParameterInfoSerializer( this._caravelaMethodInfoSerializer ).Serialize( new CompileTimeReturnParameterInfo( p ) )
                                                                                                        .ToString();

            return actual;
        }
    }
}