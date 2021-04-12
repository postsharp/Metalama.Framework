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
        public void TestDiagnosticLocation()
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

            // Properties
            AssertLocation( "AutomaticProperty", type.Properties.OfName( "AutomaticProperty" ).Single().GetDiagnosticLocation() );
            var property = type.Properties.OfName( "Property" ).Single();
            AssertLocation( "get", property.Getter!.GetDiagnosticLocation() );
            AssertLocation( "set", property.Setter!.GetDiagnosticLocation() );

            // Fields
            AssertLocation( "field1", type.Properties.OfName( "field1" ).Single().GetDiagnosticLocation() );
            
            // Attributes
            AssertLocation( "NonSerialized", type.Properties.OfName( "field1" ).Single().Attributes.Single().GetDiagnosticLocation() );
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