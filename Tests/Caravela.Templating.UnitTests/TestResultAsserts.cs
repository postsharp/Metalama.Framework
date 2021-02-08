﻿using System;
using System.Linq;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public static class TestResultAsserts
    {
        public static void AssertOutput( this TestResult testResult, string expectedOutput )
        {
            Assert.Null( testResult.TestErrorMessage );
            Assert.Null( testResult.TestException );

            // Don't test output if we have an error.
            if ( testResult.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                Assert.False( true, string.Join( Environment.NewLine, testResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.GetMessage() ) ) );
            }

            Assert.Equal( expectedOutput.Trim(), testResult.TemplateOutputSource?.ToString()?.Trim() );
        }

        public static void AssertDiagnosticId( this TestResult testResult, string expectedId )
        {
            Assert.Contains( testResult.Diagnostics, d => d.Id.Equals( expectedId, StringComparison.Ordinal ) );
        }
    }
}
