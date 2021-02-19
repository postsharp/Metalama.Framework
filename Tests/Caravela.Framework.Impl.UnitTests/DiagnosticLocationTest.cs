using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class DiagnosticTests : TestBase
    {

        [Fact]
        public void TestLocations()
        {
            
            var code = @"
class C<T> : object
{
    static C() {}
    public C() {}
    int field = 0;
    void Method<M>(int parameter) {}
    int AutomaticProperty { get; set; }
    int Property { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
}
";

            var compilation = CreateCompilation( code );

            var type = compilation.DeclaredTypes.Single();
            var method = type.Methods.Named( "Method" ).Single();
            
            // Type
            AssertLocation( "C", type.GetLocation() );
            AssertLocation( "T", type.GenericParameters.Single().GetLocation() );

            // Constructors
            AssertLocation( "C", type.Constructors.Single().GetLocation() );
            AssertLocation( "C", type.StaticConstructor!.GetLocation() );

            // Methods
            AssertLocation( "Method", method.GetLocation() );
            AssertLocation( "M", method.GenericParameters.Single().GetLocation() );
            AssertLocation( "parameter", method.Parameters.Single().GetLocation() );
            AssertLocation( "Method", method.ReturnParameter.GetLocation() );
            
            // Properties
            AssertLocation( "AutomaticProperty", type.Properties.Named( "AutomaticProperty" ).Single().GetLocation() );
            var property = type.Properties.Named( "Property" ).Single();
            AssertLocation( "get", property.Getter!.GetLocation() );
            AssertLocation( "set", property.Setter!.GetLocation() );
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