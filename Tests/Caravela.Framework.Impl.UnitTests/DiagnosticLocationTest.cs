using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class DiagnosticTests : TestBase
    {

        [Fact]
        public void TestLocations()
        {
            
            var code = @"
class C : object
{
    int field = 0;
    void Method() {};
    int Property { get; set; }
}
";

            var compilation = CreateCompilation( code );
            
            
        }
        
        
        
        
    }
}