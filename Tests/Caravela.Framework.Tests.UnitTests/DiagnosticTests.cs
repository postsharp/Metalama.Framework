// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
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
        
        [Fact]
        public void TestLocationForDiagnosticSuppression()
        {
            var code = @"
using System;
using System.Runtime.InteropServices;
class C<T> : object
{
    static C() {}
    public C() {}
    [NonSerialized]
    int field1 = 0, field2;
    void Method<M>([Out] int parameter) {}
    int AutomaticProperty { get; set; }
    int Property { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes.Single();
            var method = type.Methods.OfName( "Method" ).Single();

            // Type
            AssertLocation( "T", type.GenericParameters.Single().GetLocationsForDiagnosticSuppression() );

            // Constructors
            AssertLocation( "public C() {}", type.Constructors.Single().GetLocationsForDiagnosticSuppression() );
            AssertLocation( "static C() {}", type.StaticConstructor!.GetLocationsForDiagnosticSuppression() );

            // Methods
            AssertLocation( "void Method<M>([Out] int parameter) {}", method.GetLocationsForDiagnosticSuppression() );
            AssertLocation( "M", method.GenericParameters.Single().GetLocationsForDiagnosticSuppression() );
            AssertLocation( "[Out] int parameter", method.Parameters.Single().GetLocationsForDiagnosticSuppression() );
            
            // Properties
            AssertLocation( "int AutomaticProperty { get; set; }", type.Properties.OfName( "AutomaticProperty" ).Single().GetLocationsForDiagnosticSuppression() );
            var property = type.Properties.OfName( "Property" ).Single();
            AssertLocation( "get => throw new System.NotImplementedException();", property.Getter!.GetLocationsForDiagnosticSuppression() );
            AssertLocation( "set => throw new System.NotImplementedException();", property.Setter!.GetLocationsForDiagnosticSuppression() );

            // Fields
            AssertLocation( "field1 = 0", type.Properties.OfName( "field1" ).Single().GetLocationsForDiagnosticSuppression() );
            
            // Attributes
            AssertLocation( "NonSerialized", type.Properties.OfName( "field1" ).Single().Attributes.Single().GetLocationForDiagnosticReport() );
        }

        private static void AssertLocation( string? expectedText, IEnumerable<Location> location )
            => AssertLocation( expectedText, location.SingleOrDefault() );
        
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