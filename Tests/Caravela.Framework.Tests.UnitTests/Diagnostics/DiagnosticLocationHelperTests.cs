// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Tests.UnitTests.Diagnostics
{
    public class DiagnosticLocationHelperTests : TestBase
    {
        [Fact]
        public void GetLocation()
        {
            var code = @"
using System;
class C<T> : object
{
    static C() {}
    public C() {}
    [NonSerialized]
    int field1 = 0, field2;
    void Method<M>(int parameter) {}
    int AutomaticProperty { get; set; }
    int Property { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    ~C() {}
    public static C<T> operator + ( C<T> a, C<T> b )  => throw new System.NotImplementedException();
    public static implicit operator int(C<T> a) => 0;
    public int this[int i] => 0;
}
";

            var compilation = CreateCompilationModel( code );

            var type = compilation.DeclaredTypes.Single();
            var method = type.Methods.OfName( "Method" ).Single();

            // Type
            AssertLocation( "C", type.GetDiagnosticLocation() );
            AssertLocation( "T", type.GenericParameters.Single().GetDiagnosticLocation() );

            // Constructors
            AssertLocation( "C", type.Constructors.Single().GetDiagnosticLocation() );
            AssertLocation( "C", type.StaticConstructor!.GetDiagnosticLocation() );

            // Methods
            AssertLocation( "Method", method.GetDiagnosticLocation() );
            AssertLocation( "M", method.GenericParameters.Single().GetDiagnosticLocation() );
            AssertLocation( "parameter", method.Parameters.Single().GetDiagnosticLocation() );
            AssertLocation( "Method", method.ReturnParameter.GetDiagnosticLocation() );

            // Operators
            AssertLocation( "operator", type.Methods.OfKind( MethodKind.UserDefinedOperator ).Single().GetDiagnosticLocation() );
            AssertLocation( "operator", type.Methods.OfKind( MethodKind.ConversionOperator ).Single().GetDiagnosticLocation() );

            // Destructors
            AssertLocation( "C", type.Methods.OfKind( MethodKind.Finalizer ).Single().GetDiagnosticLocation() );

            // Properties
            AssertLocation( "AutomaticProperty", type.Properties.OfName( "AutomaticProperty" ).Single().GetDiagnosticLocation() );
            var property = type.Properties.OfName( "Property" ).Single();
            AssertLocation( "get", property.GetMethod!.GetDiagnosticLocation() );
            AssertLocation( "set", property.SetMethod!.GetDiagnosticLocation() );

            // Indexer
            AssertLocation( "this", type.Properties.OfName( "this[]" ).Single().GetDiagnosticLocation() );

            // Fields
            AssertLocation( "field1", type.Fields.OfName( "field1" ).Single().GetDiagnosticLocation() );

            // Attributes
            AssertLocation( "NonSerialized", type.Fields.OfName( "field1" ).Single().Attributes.Single().GetDiagnosticLocation() );
        }

        private static void AssertLocation( string? expectedText, Location? location )
        {
            if ( expectedText == null )
            {
                Assert.Null( location );

                return;
            }

            Assert.NotNull( location );

            var actualText = location!.SourceTree!.GetText().GetSubText( location.SourceSpan ).ToString();

            Assert.Equal( expectedText, actualText );
        }
    }
}