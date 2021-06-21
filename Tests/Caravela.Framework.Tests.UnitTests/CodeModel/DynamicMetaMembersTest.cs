// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class DynamicMetaMembersTest : TestBase
    {
        [Fact]
        public void Methods()
        {
            var code = @"
class TargetCode
{
    void ToString(string format) {}
    
    static void Foo() {}

    void Method()
    {
        void Local() {}
    }

    static void ByRef( out int o, ref string s )
    {
        o = 5;
    }

}";

            var generator = LanguageServiceFactory.CSharpSyntaxGenerator;

            var compilation = CreateCompilationModel( code );

            var type = compilation.DeclaredTypes[0];
            var toString = type.Methods.OfName( "ToString" ).Single();
            var fooMethod = type.Methods.OfName( "Foo" ).Single();
            var byRefMethod = type.Methods.OfName( "ByRef" ).Single();

            // Test normal case.
            AssertEx.DynamicEquals(
                toString.Invokers.Final.Invoke(
                    new RuntimeExpression( generator.ThisExpression() ),
                    new RuntimeExpression( generator.LiteralExpression( "x" ) ) ),
                @"((global::TargetCode)(this)).ToString((global::System.String)(""x""))" );

            AssertEx.DynamicEquals(
                toString.Invokers.Final.Invoke(
                    new RuntimeExpression( generator.LiteralExpression( 42 ) ),
                    new RuntimeExpression( generator.LiteralExpression( 43 ) ) ),
                @"((global::TargetCode)(42)).ToString((global::System.String)(43))" );

            // Test static call.
            AssertEx.DynamicEquals(
                fooMethod.Invokers.Final.Invoke( null ),
                @"global::TargetCode.Foo()" );

            // Test exception related to the 'instance' parameter.
            AssertEx.ThrowsWithDiagnostic(
                GeneralDiagnosticDescriptors.CannotProvideInstanceForStaticMember,
                () => fooMethod.Invokers.Final.Invoke( new RuntimeExpression( generator.LiteralExpression( 42 ) ) ) );

            AssertEx.ThrowsWithDiagnostic(
                GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember,
                () => toString.Invokers.Final.Invoke(
                    null,
                    new RuntimeExpression( generator.LiteralExpression( "x" ) ) ) );

            // Test in/out.
            var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

            AssertEx.DynamicEquals(
                byRefMethod.Invokers.Final.Invoke(
                    null,
                    new RuntimeExpression( generator.IdentifierName( "x" ), intType, true ),
                    new RuntimeExpression( generator.IdentifierName( "y" ), intType, true ) ),
                @"global::TargetCode.ByRef(out x, ref y)" );

            AssertEx.ThrowsWithDiagnostic(
                GeneralDiagnosticDescriptors.CannotPassExpressionToByRefParameter,
                () => byRefMethod.Invokers.Final.Invoke(
                    null,
                    new RuntimeExpression( generator.IdentifierName( "x" ) ),
                    new RuntimeExpression( generator.IdentifierName( "y" ) ) ) );
        }

        [Fact]
        public void Generics()
        {
            var code = @"
class TargetCode
{
    class Nested<T1> {
        static void Foo<T2>() {}
    }

    void Method()
    {
    }
}";

            var compilation = CreateCompilationModel( code );

            var type = compilation.DeclaredTypes.OfName( "TargetCode" ).Single();
            var nestedType = type.NestedTypes.Single().WithGenericArguments( compilation.Factory.GetTypeByReflectionType( typeof(string) )! );
            var method = nestedType.Methods.Single().WithGenericArguments( compilation.Factory.GetTypeByReflectionType( typeof(int) )! );

            AssertEx.DynamicEquals(
                method.Invokers.Final.Invoke( null ),
                @"global::TargetCode.Nested<global::System.String>.Foo<global::System.Int32>()" );
        }

        [Fact]
        public void LocalFunctions()
        {
            var code = @"
class TargetCode
{
    void Method()
    {
        void Local() {}
    }
}";

            var compilation = CreateCompilationModel( code );
            var localFunction = compilation.DeclaredTypes.OfName( "TargetCode" ).Single().Methods.Single().LocalFunctions.Single();

            AssertEx.DynamicEquals(
                localFunction.Invokers.Final.Invoke( null ),
                @"Local()" );

            AssertEx.ThrowsWithDiagnostic(
                GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction,
                () => localFunction.Invokers.Final.Invoke( new RuntimeExpression( SyntaxFactory.ThisExpression() ) ) );
        }

        [Fact]
        public void Parameters()
        {
            var code = @"
class TargetCode
{
    void Method( ref int i, ref long j )
    {
    }
}";

            var compilation = CreateCompilationModel( code );
            var method = compilation.DeclaredTypes.Single().Methods.Single();

            AdviceParameterList adviceParameterList = new( method );

            // ReSharper disable once IDE0058
            AssertEx.DynamicEquals( adviceParameterList[0].Value, @"i" );

            // ReSharper disable once IDE0058
            AssertEx.DynamicEquals( adviceParameterList[1].Value, @"j" );

            Assert.Equal( adviceParameterList[0], adviceParameterList["i"] );
            Assert.Equal( adviceParameterList[1], adviceParameterList["j"] );

            Assert.Equal( "i", Assert.Single( adviceParameterList.OfType( typeof(int) ) )!.Name );
        }

        [Fact]
        public void Properties()
        {
            var code = @"
class TargetCode
{
    TargetCode P { get; set; }
    int this[int index] => 42;
}";

            var compilation = CreateCompilationModel( code );

            var type = compilation.DeclaredTypes.Single();
            var property = type.Properties.OfName( "P" ).Single();
            RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression() );

            AssertEx.DynamicEquals( property.Invokers.Final.GetValue( thisExpression ), @"((global::TargetCode)(this)).P" );

            AssertEx.DynamicEquals(
                property.Invokers.Final.GetValue( property.Invokers.Final.GetValue( thisExpression ) ),
                @"((global::TargetCode)(this)).P.P" );
        }

        [Fact]
        public void ToArrayTest()
        {
            var code = @"
class TargetCode
{
    void A( int a, string b, object c, out System.DateTime d, ref System.TimeSpan e)
    {
        d = default;
    }

    void B( int a, int b, int c, int d, int e, int f, int g, int h, int i, int j, int k, int l)
    {
    }

    void C()
    {
    }
}";

            var compilation = CreateCompilationModel( code );
            var type = compilation.DeclaredTypes.Single();
            var method = type.Methods.OfName( "A" ).Single();
            var longMethod = type.Methods.OfName( "B" ).Single();
            var noParameterMethod = type.Methods.OfName( "C" ).Single();

            AssertEx.DynamicEquals(
                new AdviceParameterList( method ).Values.ToArray(),
                @"new object[]{a, b, c, default(global::System.DateTime), e}" );

            AssertEx.DynamicEquals(
                new AdviceParameterList( noParameterMethod ).Values.ToArray(),
                @"new object[]{}" );

            AssertEx.DynamicEquals(
                new AdviceParameterList( method ).Values.ToValueTuple(),
                @"(a, b, c, default(global::System.DateTime), e)" );

            AssertEx.DynamicEquals(
                new AdviceParameterList( longMethod ).Values.ToValueTuple(),
                @"(a, b, c, d, e, f, g, h, i, j, k, l)" );

            AssertEx.DynamicEquals(
                new AdviceParameterList( noParameterMethod ).Values.ToValueTuple(),
                @"default(global::System.ValueType)" );
        }
    }
}