// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CodeModel
{
    public class DiagnosticTests : TestBase
    {
        [Fact]
        public void TestLocationForDiagnosticReport()
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
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes.Single();
            var method = type.Methods.OfName( "Method" ).Single();

            // Type
            AssertLocation( "C", type.GetLocationForDiagnosticReport() );
            AssertLocation( "T", type.GenericParameters.Single().GetLocationForDiagnosticReport() );

            // Constructors
            AssertLocation( "C", type.Constructors.Single().GetLocationForDiagnosticReport() );
            AssertLocation( "C", type.StaticConstructor!.GetLocationForDiagnosticReport() );

            // Methods
            AssertLocation( "Method", method.GetLocationForDiagnosticReport() );
            AssertLocation( "M", method.GenericParameters.Single().GetLocationForDiagnosticReport() );
            AssertLocation( "parameter", method.Parameters.Single().GetLocationForDiagnosticReport() );
            AssertLocation( "Method", method.ReturnParameter.GetLocationForDiagnosticReport() );

            // Properties
            AssertLocation( "AutomaticProperty", type.Properties.OfName( "AutomaticProperty" ).Single().GetLocationForDiagnosticReport() );
            var property = type.Properties.OfName( "Property" ).Single();
            AssertLocation( "get", property.Getter!.GetLocationForDiagnosticReport() );
            AssertLocation( "set", property.Setter!.GetLocationForDiagnosticReport() );

            // Fields
            AssertLocation( "field1", type.Properties.OfName( "field1" ).Single().GetLocationForDiagnosticReport() );
            
            // Attributes
            AssertLocation( "NonSerialized", type.Properties.OfName( "field1" ).Single().Attributes.Single().GetLocationForDiagnosticReport() );
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

        [Fact]
        public void TestValueTupleAdapter()
        {
            Assert.Equal( new object[] { 1, "2" }, ValueTupleAdapter.ToArray( (1, "2") ) );
        }
    }
}