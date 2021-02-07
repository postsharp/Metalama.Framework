using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;
using Xunit;

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

        public static void AssertTransformedSource( this TestResult testResult, string expectedTransformedSource )
        {
            // Don't test output if we have an error.
            testResult.AssertNoErrors();

            Assert.Equal( expectedTransformedSource.Trim(), testResult.TransformedTargetSource?.ToString()?.Trim() );
        }

        public static void AssertTransformedSourceSpan( this TestResult testResult, string expectedTransformedSource, TextSpan textSpan )
        {
            // Don't test output if we have an error.
            testResult.AssertNoErrors();

            string regionText = testResult.TransformedTargetSource.GetSubText( textSpan ).ToString()?.Trim();
            Assert.Equal( expectedTransformedSource.Trim(), regionText );
        }

        public static void AssertDiagnosticId( this TestResult testResult, string expectedId )
        {
            Assert.Contains( testResult.Diagnostics, d => d.Id.Equals( expectedId ) );
        }
    }
}
