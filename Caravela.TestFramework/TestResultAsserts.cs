// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Sdk;

namespace Caravela.TestFramework
{
    public static class TestResultAsserts
    {
        public static void AssertNoErrors( this TestResult testResult )
        {
            Assert.Null( testResult.ErrorMessage );
            Assert.Null( testResult.Exception );
            Assert.True( testResult.Success );

            if ( testResult.Diagnostics.Any( d => d.Severity == DiagnosticSeverity.Error ) )
            {
                Assert.False( true, string.Join( Environment.NewLine, testResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.GetMessage() ) ) );
            }
        }

        public static void AssertTransformedSourceSpanEqual( this TestResult testResult, string expectedTransformedSource, TextSpan? textSpan, string? actualOutputPath = null )
        {
            var regionText = textSpan != null
                ? testResult.TransformedTargetSource?.GetSubText( textSpan.Value ).ToString()
                : testResult.TransformedTargetSource?.ToString();
            AssertSourceEqual( expectedTransformedSource.Trim(), regionText, actualOutputPath );
        }

        public static void AssertTransformedSourceSpanEqual( this TestResult testResult, string expectedTransformedSource, string actualTransformedSource, string? actualOutputPath = null )
        {
            AssertSourceEqual( expectedTransformedSource.Trim(), actualTransformedSource, actualOutputPath );
        }

        private static void AssertSourceEqual( string expected, string? actual, string? actualOutputPath = null )
        {
            try
            {
                Assert.Equal( expected.Trim(), actual?.Trim() );
            }
            catch ( EqualException )
            {
                if ( actualOutputPath != null )
                {
                    Directory.CreateDirectory( Path.GetDirectoryName( actualOutputPath ) );
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
