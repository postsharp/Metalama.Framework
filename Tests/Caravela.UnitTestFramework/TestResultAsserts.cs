using System;
using System.IO;
using System.Linq;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.UnitTestFramework
{
    public static class TestResultAsserts
    {
        public static void AssertTransformedSourceEquals( this TestResult testResult, string expectedOutput, string? saveActualOutputPath = null )
        {
            // Don't test output if we have an error.
            Assert.Null( testResult.TestErrorMessage );
            if ( testResult.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                Assert.False( true, string.Join( Environment.NewLine, testResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.GetMessage() ) ) );
            }

            var actualOutput = testResult.TemplateOutputSource?.ToString()?.Trim();
            if ( expectedOutput.Trim() != actualOutput )
            {
                if ( saveActualOutputPath != null && actualOutput != null )
                {
                    File.WriteAllText( saveActualOutputPath, actualOutput );
                }

                Assert.Equal( expectedOutput.Trim(), actualOutput );
            }
        }

        public static void AssertContainsDiagnosticId( this TestResult testResult, string expectedId )
        {
            Assert.Contains( testResult.Diagnostics, d => d.Id.Equals( expectedId, StringComparison.Ordinal ) );
        }
    }
}
