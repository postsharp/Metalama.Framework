// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
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

            var generator = OurSyntaxGenerator.Default;

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types[0];
                var toString = type.Methods.OfName( "ToString" ).Single();
                var fooMethod = type.Methods.OfName( "Foo" ).Single();
                var byRefMethod = type.Methods.OfName( "ByRef" ).Single();

                // Test normal case.
                AssertEx.DynamicEquals(
                    toString.Invokers.Final.Invoke(
                        new RuntimeExpression( generator.ThisExpression(), compilation, serviceProvider ),
                        new RuntimeExpression( generator.LiteralExpression( "x" ), compilation, serviceProvider ) ),
                    @"((global::TargetCode)this).ToString((global::System.String)""x"")" );

                AssertEx.DynamicEquals(
                    toString.Invokers.ConditionalFinal.Invoke(
                        new RuntimeExpression( generator.IdentifierName( "a" ), compilation, serviceProvider ),
                        new RuntimeExpression( generator.LiteralExpression( "x" ), compilation, serviceProvider ) ),
                    @"((global::TargetCode)a)?.ToString((global::System.String)""x"")" );

                AssertEx.DynamicEquals(
                    toString.Invokers.Final.Invoke(
                        new RuntimeExpression( generator.LiteralExpression( 42 ), compilation, serviceProvider ),
                        new RuntimeExpression( generator.LiteralExpression( 43 ), compilation, serviceProvider ) ),
                    @"((global::TargetCode)42).ToString((global::System.String)43)" );

                // Test static call.
                AssertEx.DynamicEquals(
                    fooMethod.Invokers.Final.Invoke( null ),
                    @"global::TargetCode.Foo()" );

                // Test exception related to the 'instance' parameter.
                AssertEx.DynamicEquals(
                    fooMethod.Invokers.Final.Invoke( new RuntimeExpression( SyntaxFactoryEx.Null, compilation, serviceProvider ) ),
                    @"global::TargetCode.Foo()" );

                AssertEx.ThrowsWithDiagnostic(
                    GeneralDiagnosticDescriptors.MustProvideInstanceForInstanceMember,
                    () => toString.Invokers.Final.Invoke(
                        null,
                        new RuntimeExpression( generator.LiteralExpression( "x" ), compilation, serviceProvider ) ) );

                // Test in/out.
                var intType = compilation.Factory.GetTypeByReflectionType( typeof(int) );

                AssertEx.DynamicEquals(
                    byRefMethod.Invokers.Final.Invoke(
                        null,
                        new RuntimeExpression( generator.IdentifierName( "x" ), intType, SyntaxGenerationContext.CreateDefault( serviceProvider ), true ),
                        new RuntimeExpression( generator.IdentifierName( "y" ), intType, SyntaxGenerationContext.CreateDefault( serviceProvider ), true ) ),
                    @"global::TargetCode.ByRef(out x, ref y)" );

                AssertEx.ThrowsWithDiagnostic(
                    GeneralDiagnosticDescriptors.CannotPassExpressionToByRefParameter,
                    () => byRefMethod.Invokers.Final.Invoke(
                        null,
                        new RuntimeExpression( generator.IdentifierName( "x" ), compilation, serviceProvider ),
                        new RuntimeExpression( generator.IdentifierName( "y" ), compilation, serviceProvider ) ) );
            }
        }

        [Fact]
        public void OpenGenerics()
        {
            var code = @"
class TargetCode
{
    class Nested<T1> {
        static void StaticGenericMethod<T2>() {}
        static void StaticNonGenericMethod() {}
        static int StaticField;
        static string StaticProperty {get;set;}
        static event System.Action<object> StaticEvent;
        void InstanceGenericMethod<T2>() {}
        void InstanceNonGenericMethod() {}
        int InstanceField;
        string InstanceProperty {get;set;}
        event System.Action<object> InstanceEvent;
    }

    void Method()
    {
    }
}";

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types.OfName( "TargetCode" ).Single();
                var nestedType = type.NestedTypes.Single();

                // Testing static members.
                var staticGenericMethod = nestedType.Methods.OfName( "StaticGenericMethod" ).Single();
                var staticNonGenericMethod = nestedType.Methods.OfName( "StaticNonGenericMethod" ).Single();
                var staticField = nestedType.Fields.OfName( "StaticField" ).Single();
                var staticProperty = nestedType.Properties.OfName( "StaticProperty" ).Single();
                var staticEvent = nestedType.Events.OfName( "StaticEvent" ).Single();

                AssertEx.DynamicThrows<InvalidOperationException>( () => staticGenericMethod.Invokers.Final.Invoke( null ) );

                AssertEx.DynamicThrows<InvalidOperationException>( () => staticNonGenericMethod.Invokers.Final.Invoke( null ) );

                AssertEx.DynamicThrows<InvalidOperationException>( () => staticField.Invokers.Final.GetValue( null ) );
                AssertEx.DynamicThrows<InvalidOperationException>( () => staticProperty.Invokers.Final.GetValue( null ) );
                AssertEx.DynamicThrows<InvalidOperationException>( () => staticEvent.Invokers.Final.Add( null, null ) );

                // Testing instance members on a generic type.
                var instance = new RuntimeExpression( SyntaxFactory.ParseExpression( "abc" ), compilation, serviceProvider );
                var instanceGenericMethod = nestedType.Methods.OfName( "InstanceGenericMethod" ).Single();
                var instanceNonGenericMethod = nestedType.Methods.OfName( "InstanceNonGenericMethod" ).Single();
                var instanceField = nestedType.Fields.OfName( "InstanceField" ).Single();
                var instanceProperty = nestedType.Properties.OfName( "InstanceProperty" ).Single();
                var instanceEvent = nestedType.Events.OfName( "InstanceEvent" ).Single();

                AssertEx.DynamicThrows<InvalidOperationException>( () => instanceGenericMethod.Invokers.Final.Invoke( instance ) );

                AssertEx.DynamicThrows<InvalidOperationException>( () => instanceNonGenericMethod.Invokers.Final.Invoke( instance ) );

                AssertEx.DynamicThrows<InvalidOperationException>( () => instanceField.Invokers.Final.GetValue( instance ) );
                AssertEx.DynamicThrows<InvalidOperationException>( () => instanceProperty.Invokers.Final.GetValue( instance ) );
                AssertEx.DynamicThrows<InvalidOperationException>( () => instanceEvent.Invokers.Final.Add( instance, null ) );
            }
        }

        [Fact]
        public void Generics()
        {
            var code = @"
class TargetCode
{
    class Nested<T1> {
        static void StaticGenericMethod<T2>() {}
        static void StaticNonGenericMethod() {}
        static int StaticField;
        static string StaticProperty {get;set;}
        static event System.Action<object> StaticEvent;
        void InstanceGenericMethod<T2>() {}
        void InstanceNonGenericMethod() {}
        int InstanceField;
        string InstanceProperty {get;set;}
        event System.Action<object> InstanceEvent;
    }

    void Method()
    {
    }
}";

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types.OfName( "TargetCode" ).Single();
                var nestedType = type.NestedTypes.Single().ConstructGenericInstance( compilation.Factory.GetTypeByReflectionType( typeof(string) ) );

                // Testing static members.
                var staticGenericMethod = nestedType.Methods.OfName( "StaticGenericMethod" )
                    .Single()
                    .ConstructGenericInstance( compilation.Factory.GetTypeByReflectionType( typeof(int) ) );

                var staticNonGenericMethod = nestedType.Methods.OfName( "StaticNonGenericMethod" ).Single();
                var staticField = nestedType.Fields.OfName( "StaticField" ).Single();
                var staticProperty = nestedType.Properties.OfName( "StaticProperty" ).Single();
                var staticEvent = nestedType.Events.OfName( "StaticEvent" ).Single();

                AssertEx.DynamicEquals(
                    staticGenericMethod.Invokers.Final.Invoke( null ),
                    @"global::TargetCode.Nested<global::System.String>.StaticGenericMethod<global::System.Int32>()" );

                AssertEx.DynamicEquals(
                    staticNonGenericMethod.Invokers.Final.Invoke( null ),
                    @"global::TargetCode.Nested<global::System.String>.StaticNonGenericMethod()" );

                AssertEx.DynamicEquals( staticField.Invokers.Final.GetValue( null ), "global::TargetCode.Nested<global::System.String>.StaticField" );
                AssertEx.DynamicEquals( staticProperty.Invokers.Final.GetValue( null ), "global::TargetCode.Nested<global::System.String>.StaticProperty" );
                AssertEx.DynamicEquals( staticEvent.Invokers.Final.Add( null, null ), "global::TargetCode.Nested<global::System.String>.StaticEvent += null" );

                // Testing instance members on a generic type.
                var instance = new RuntimeExpression( SyntaxFactory.ParseExpression( "abc" ), compilation, serviceProvider );

                var instanceGenericMethod = nestedType.Methods.OfName( "InstanceGenericMethod" )
                    .Single()
                    .ConstructGenericInstance( compilation.Factory.GetTypeByReflectionType( typeof(int) ) );

                var instanceNonGenericMethod = nestedType.Methods.OfName( "InstanceNonGenericMethod" ).Single();
                var instanceField = nestedType.Fields.OfName( "InstanceField" ).Single();
                var instanceProperty = nestedType.Properties.OfName( "InstanceProperty" ).Single();
                var instanceEvent = nestedType.Events.OfName( "InstanceEvent" ).Single();

                AssertEx.DynamicEquals(
                    instanceGenericMethod.Invokers.Final.Invoke( instance ),
                    @"((global::TargetCode.Nested<global::System.String>)abc).InstanceGenericMethod<global::System.Int32>()" );

                AssertEx.DynamicEquals(
                    instanceNonGenericMethod.Invokers.Final.Invoke( instance ),
                    @"((global::TargetCode.Nested<global::System.String>)abc).InstanceNonGenericMethod()" );

                AssertEx.DynamicEquals(
                    instanceField.Invokers.Final.GetValue( instance ),
                    "((global::TargetCode.Nested<global::System.String>)abc).InstanceField" );

                AssertEx.DynamicEquals(
                    instanceProperty.Invokers.Final.GetValue( instance ),
                    "((global::TargetCode.Nested<global::System.String>)abc).InstanceProperty" );

                AssertEx.DynamicEquals(
                    instanceEvent.Invokers.Final.Add( instance, null ),
                    "((global::TargetCode.Nested<global::System.String>)abc).InstanceEvent += null" );
            }
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

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var localFunction = compilation.Types.OfName( "TargetCode" ).Single().Methods.Single().LocalFunctions.Single();

                AssertEx.DynamicEquals(
                    localFunction.Invokers.Final.Invoke( null ),
                    @"Local()" );

                AssertEx.ThrowsWithDiagnostic(
                    GeneralDiagnosticDescriptors.CannotProvideInstanceForLocalFunction,
                    () => localFunction.Invokers.Final.Invoke( new RuntimeExpression( SyntaxFactory.ThisExpression(), compilation, serviceProvider ) ) );
            }
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

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var method = compilation.Types.Single().Methods.Single();

                AdvisedParameterList advisedParameterList = new( method );

                // ReSharper disable once IDE0058
                AssertEx.DynamicEquals( advisedParameterList[0].Value, @"i" );

                // ReSharper disable once IDE0058
                AssertEx.DynamicEquals( advisedParameterList[1].Value, @"j" );

                Assert.Equal( advisedParameterList[0], advisedParameterList["i"] );
                Assert.Equal( advisedParameterList[1], advisedParameterList["j"] );

                Assert.Equal( "i", Assert.Single( advisedParameterList.OfType( typeof(int) ) )!.Name );
            }
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

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types.Single();
                var property = type.Properties.OfName( "P" ).Single();
                RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation, serviceProvider );

                AssertEx.DynamicEquals( property.Invokers.Final.GetValue( thisExpression ), @"((global::TargetCode)this).P" );

                AssertEx.DynamicEquals(
                    property.Invokers.ConditionalFinal.GetValue( SyntaxFactory.IdentifierName( "a" ) ),
                    @"((global::TargetCode)a)?.P" );

                AssertEx.DynamicEquals(
                    property.Invokers.Final.SetValue( SyntaxFactory.IdentifierName( "a" ), SyntaxFactory.IdentifierName( "b" ) ),
                    @"((global::TargetCode)a).P = b" );

#if NET5_0_OR_GREATER
                AssertEx.DynamicEquals(
                    property.Invokers.Final.GetValue( property.Invokers.Final.GetValue( thisExpression ) ),
                    @"((global::TargetCode)this).P.P" );
#else
                /*
                 * There is a weird exception in .NET Framework because of the dynamic binder, but this should not affect any production scenario.
                 */
#endif
            }
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

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types.Single();
                var property = type.Properties.OfName( "P" ).Single();
                RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation, serviceProvider );

                AssertEx.DynamicEquals( property.Invokers.Final.GetValue( thisExpression ), @"((global::TargetCode)this).P" );

                AssertEx.DynamicEquals(
                    property.GetMethod!.Invokers.ConditionalFinal.Invoke( SyntaxFactory.IdentifierName( "a" ) ),
                    @"((global::TargetCode)a)?.P" );

                AssertEx.DynamicEquals(
                    property.GetMethod!.Invokers.Final.Invoke( property.Invokers.Final.GetValue( thisExpression ) ),
                    @"((global::TargetCode)this).P.P" );
            }
        }

        [Fact]
        public void Events()
        {
            var code = @"
class TargetCode
{
    event System.EventHandler MyEvent;
}";

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types.Single();
                var @event = type.Events.Single();

                RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation, serviceProvider );
                RuntimeExpression parameterExpression = new( SyntaxFactory.IdentifierName( "value" ), compilation, serviceProvider );

                AssertEx.DynamicEquals( @event.Invokers.Final.Add( thisExpression, parameterExpression ), @"((global::TargetCode)this).MyEvent += value" );
                AssertEx.DynamicEquals( @event.Invokers.Final.Remove( thisExpression, parameterExpression ), @"((global::TargetCode)this).MyEvent -= value" );

#if NET5_0_OR_GREATER
                AssertEx.DynamicEquals(
                    @event.Invokers.Final.Raise( thisExpression, parameterExpression, parameterExpression ),
                    @"((global::TargetCode)this).MyEvent?.Invoke((global::System.Object? )value, (global::System.EventArgs)value)" );
#else
                AssertEx.DynamicEquals(
                    @event.Invokers.Final.Raise( thisExpression, parameterExpression, parameterExpression ),
                    @"((global::TargetCode)this).MyEvent?.Invoke((global::System.Object)value, (global::System.EventArgs)value)" );
#endif
            }
        }

        [Fact]
        public void EventAccessors()
        {
            var code = @"
class TargetCode
{
    event System.EventHandler MyEvent;
}";

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types.Single();
                var @event = type.Events.Single();

                RuntimeExpression thisExpression = new( SyntaxFactory.ThisExpression(), compilation, serviceProvider );
                RuntimeExpression parameterExpression = new( SyntaxFactory.IdentifierName( "value" ), compilation, serviceProvider );

                AssertEx.DynamicEquals(
                    @event.AddMethod.Invokers.Final.Invoke( thisExpression, parameterExpression ),
                    @"((global::TargetCode)this).MyEvent += value" );

                AssertEx.DynamicEquals(
                    @event.RemoveMethod.Invokers.Final.Invoke( thisExpression, parameterExpression ),
                    @"((global::TargetCode)this).MyEvent -= value" );

#if NET5_0_OR_GREATER
                AssertEx.DynamicEquals(
                    @event.RaiseMethod?.Invokers.Final.Invoke( thisExpression, parameterExpression, parameterExpression ),
                    @"((global::TargetCode)this).MyEvent?.Invoke((global::System.Object? )value, (global::System.EventArgs)value)" );
#else
                AssertEx.DynamicEquals(
                    @event.RaiseMethod?.Invokers.Final.Invoke( thisExpression, parameterExpression, parameterExpression ),
                    @"((global::TargetCode)this).MyEvent?.Invoke((global::System.Object)value, (global::System.EventArgs)value)" );
#endif
            }
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

            using var testContext = this.CreateTestContext();
            var serviceProvider = testContext.ServiceProvider;

            var compilation = testContext.CreateCompilationModel( code );

            using ( TemplateExpansionContext.WithTestingContext(
                       SyntaxGenerationContext.CreateDefault( serviceProvider, compilation.RoslynCompilation ),
                       serviceProvider ) )
            {
                var type = compilation.Types.Single();
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
}