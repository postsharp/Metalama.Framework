using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public static class TestResultAsserts
    {
        public static void AssertOutput( this TestResult testResult, string expectedOuput )
        {
            Assert.Equal( expectedOuput.Trim(), testResult.TemplateOutputSource?.ToString()?.Trim() );
        }

        public static void AssertDiagnosticId( this TestResult testResult, string expectedId )
        {
            Assert.Contains( testResult.Diagnostics, d => d.Id.Equals( expectedId ) );
        }
    }
}
