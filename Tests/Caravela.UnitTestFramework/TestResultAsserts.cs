using System;
using System.IO;
using System.Linq;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Sdk;

namespace Caravela.UnitTestFramework
{
    public static class TestResultAsserts
    {
        public static void AssertNoErrors( this TestResult testResult)
        {
            Assert.Null( testResult.ErrorMessage );
            Assert.Null( testResult.Exception );

            if ( testResult.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                Assert.False( true, string.Join( Environment.NewLine, testResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.GetMessage() ) ) );
            }
        }

        public static void AssertTransformedSourceEqual( this TestResult testResult, string expectedTransformedSource, string? actualOutputPath = null )
        {
            testResult.AssertTransformedSourceSpanEqual( expectedTransformedSource, null, actualOutputPath );
        }

        public static void AssertTransformedSourceSpanEqual( this TestResult testResult, string expectedTransformedSource, TextSpan? textSpan, string? actualOutputPath = null )
        {
            // Don't test output if we have an error.
            testResult.AssertNoErrors();

            var regionText = textSpan != null
                ? testResult.TransformedTargetSource?.GetSubText( textSpan.Value ).ToString()?.Trim()
                : testResult.TransformedTargetSource?.ToString()?.Trim();
            AssertSourceEqual( expectedTransformedSource.Trim(), regionText, actualOutputPath );
        }

        private static void AssertSourceEqual(string expected, string? actual, string? actualOutputPath = null )
        {
            try
            {
                Assert.Equal( expected, actual );
            }
            catch ( EqualException )
            {
                if ( actualOutputPath != null )
                {
                    File.WriteAllText( actualOutputPath, actual );
                }

                throw;
            }
        }

        public static void AssertContainsDiagnosticId( this TestResult testResult, string expectedId )
        {
            Assert.Contains( testResult.Diagnostics, d => d.Id.Equals( expectedId, StringComparison.Ordinal ) );
        }
    }
}
