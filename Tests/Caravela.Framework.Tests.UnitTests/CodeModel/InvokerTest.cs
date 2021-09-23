// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class InvokerTest : TestBase
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
                    new RuntimeExpression( generator.ThisExpression(), compilation ),
                    new RuntimeExpression( generator.LiteralExpression( "x" ), compilation ) ),
                @"((global::TargetCode)(this)).ToString((global::System.String)(""x""))" );

            AssertEx.DynamicEquals(
                toString.Invokers.ConditionalFinal.Invoke(
                    new RuntimeExpression( generator.IdentifierName( "a" ), compilation ),
                    new RuntimeExpression( generator.LiteralExpression( "x" ), compilation ) ),
                @"((global::TargetCode)(a))?.ToString((global::System.String)(""x""))" );

            AssertEx.DynamicEquals(
                toString.Invokers.Final.Invoke(
                    new RuntimeExpression( generator.LiteralExpression( 42 ), compilation ),
                    new RuntimeExpression( generator.LiteralExpression( 43 ), compilation ) ),
                @"((global::TargetCode)(42)).ToString((global::System.String)(43))" );

            // Test static call.
            AssertEx.DynamicEquals(
                fooMethod.Invokers.Final.Invoke( null ),
                @"global::TargetCode.Foo()" );

            // Test exception related to the 'instance' parameter.
            AssertEx.DynamicEquals(
                fooMethod.Invokers.Final.Invoke( new RuntimeExpression( SyntaxFactoryEx.Null, compilation ) ),
                @"global::TargetCode.Foo()" );

            AssertEx.ThrowsWithDiagnostic(
                GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember,
                () => toString.Invokers.Final.Invoke(
                    null,
                    new RuntimeExpression( generator.LiteralExpression( "x" ), compilation ) ) );

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
                    new RuntimeExpression( generator.IdentifierName( "x" ), compilation ),
                    new RuntimeExpression( generator.IdentifierName( "y" ), compilation ) ) );
        }

        [Fact( Skip = "https://tp.postsharp.net/entity/28959" )]
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
            var nestedType = type.NestedTypes.Single().WithGenericArguments( compilation.Factory.GetTypeByReflectionType( typeof(string) ) );
            var method = nestedType.Methods.Single().WithGenericArguments( compilation.Factory.GetTypeByReflectionType( typeof(int) ) );

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
                () => localFunction.Invokers.Final.Invoke( new RuntimeExpression( SyntaxFactory.ThisExpression(), compilation ) ) );
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

            AdvisedParameterList advisedParameterList = new( method );

            // ReSharper disable once IDE0058
            AssertEx.DynamicEquals( advisedParameterList[0].Value, @"i" );

            // ReSharper disable once IDE0058
            AssertEx.DynamicEquals( advisedParameterList[1].Value, @"j" );

            Assert.Equal( advisedParameterList[0], advisedParameterList["i"] );
            Assert.Equal( advisedParameterList[1], advisedParameterList["j"] );

            Assert.Equal( "i", Assert.Single( advisedParameterList.OfType( typeof(int) ) )!.Name );
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
            RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation );

            AssertEx.DynamicEquals( property.Invokers.Final.GetValue( thisExpression ), @"((global::TargetCode)(this)).P" );

            AssertEx.DynamicEquals(
                property.Invokers.ConditionalFinal.GetValue( SyntaxFactory.IdentifierName( "a" ) ),
                @"((global::TargetCode)(a))?.P" );

            AssertEx.DynamicEquals(
                property.Invokers.Final.SetValue( SyntaxFactory.IdentifierName( "a" ), SyntaxFactory.IdentifierName( "b" ) ),
                @"((global::TargetCode)(a)).P = b" );

            AssertEx.DynamicEquals(
                property.Invokers.Final.GetValue( property.Invokers.Final.GetValue( thisExpression ) ),
                @"((global::TargetCode)(this)).P.P" );
        }

        [Fact]
        public void PropertyAccessors()
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
            RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation );

            AssertEx.DynamicEquals( property.Invokers.Final.GetValue( thisExpression ), @"((global::TargetCode)(this)).P" );

            AssertEx.DynamicEquals(
                property.GetMethod!.Invokers.ConditionalFinal.Invoke( SyntaxFactory.IdentifierName( "a" ) ),
                @"((global::TargetCode)(a))?.P" );

            AssertEx.DynamicEquals(
                property.GetMethod!.Invokers.Final.Invoke( property.Invokers.Final.GetValue( thisExpression ) ),
                @"((global::TargetCode)(this)).P.P" );
        }

        [Fact]
        public void Events()
        {
            var code = @"
class TargetCode
{
    event System.EventHandler MyEvent;
}";

            var compilation = CreateCompilationModel( code );

            var type = compilation.DeclaredTypes.Single();
            var @event = type.Events.Single();

            RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation );
            RuntimeExpression parameterExpression = new( SyntaxFactory.IdentifierName( "value" ), compilation );

            AssertEx.DynamicEquals( @event.Invokers.Final.Add( thisExpression, parameterExpression ), @"((global::TargetCode)(this)).MyEvent += value" );
            AssertEx.DynamicEquals( @event.Invokers.Final.Remove( thisExpression, parameterExpression ), @"((global::TargetCode)(this)).MyEvent -= value" );

            AssertEx.DynamicEquals(
                @event.Invokers.Final.Raise( thisExpression, parameterExpression, parameterExpression ),
                @"((global::TargetCode)(this)).MyEvent?.Invoke((global::System.Object? )(value), (global::System.EventArgs)(value))" );
        }

        [Fact]
        public void EventAccessors()
        {
            var code = @"
class TargetCode
{
    event System.EventHandler MyEvent;
}";

            var compilation = CreateCompilationModel( code );

            var type = compilation.DeclaredTypes.Single();
            var @event = type.Events.Single();

            RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation );
            RuntimeExpression parameterExpression = new( SyntaxFactory.IdentifierName( "value" ), compilation );

            AssertEx.DynamicEquals(
                @event.AddMethod.Invokers.Final.Invoke( thisExpression, parameterExpression ),
                @"((global::TargetCode)(this)).MyEvent += value" );

            AssertEx.DynamicEquals(
                @event.RemoveMethod.Invokers.Final.Invoke( thisExpression, parameterExpression ),
                @"((global::TargetCode)(this)).MyEvent -= value" );

            AssertEx.DynamicEquals(
                @event.RaiseMethod?.Invokers.Final.Invoke( thisExpression, parameterExpression, parameterExpression ),
                @"((global::TargetCode)(this)).MyEvent?.Invoke((global::System.Object? )(value), (global::System.EventArgs)(value))" );
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
                new AdvisedParameterList( method ).Values.ToArray(),
                @"new object[]{a, b, c, default(global::System.DateTime), e}" );

            AssertEx.DynamicEquals(
                new AdvisedParameterList( longMethod ).Values.ToArray(),
                @"new object[]{a, b, c, d, e, f, g, h, i, j, k, l}" );

            AssertEx.DynamicEquals(
                new AdvisedParameterList( noParameterMethod ).Values.ToArray(),
                @"new object[]{}" );
        }
    }
}