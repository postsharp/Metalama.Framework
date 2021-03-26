// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.Framework.Tests.Integration.Templating
{
    /// <summary>
    /// The base class for template integration tests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can use <see cref="FromDirectoryAttribute"/> to execute one test method on many test inputs.
    /// This is useful to write only one test method per category of tests.
    /// </para>
    /// </remarks>
    public abstract class TemplatingUnitTestBase : UnitTestBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplatingUnitTestBase"/> class.
        /// </summary>
        /// <param name="logger">The Xunit logger.</param>
        public TemplatingUnitTestBase( ITestOutputHelper logger ) : base( logger )
        {
        }

        /// <summary>
        /// Runs the template test with the given path of the source file.
        /// </summary>
        /// <param name="relativeTestPath">The path of the test source file relative to the project directory.</param>
        /// <returns>The result of the test execution.</returns>
        protected async Task<TestResult> RunTemplateTestAsync( string relativeTestPath )
        {
            var testSourceAbsolutePath = Path.Combine( this.ProjectDirectory, relativeTestPath );

            var usedSyntaxKindsCollector = new UsedSyntaxKindsCollector();
            var testRunner = new TemplatingTestRunner( new[] { usedSyntaxKindsCollector } );
            var testSource = await File.ReadAllTextAsync( testSourceAbsolutePath );
            var testResult = await testRunner.RunAsync( new TestInput( relativeTestPath, this.ProjectDirectory, testSource, relativeTestPath, null ) );

            this.WriteDiagnostics( testResult.Diagnostics );

            await this.WriteSyntaxCoverageAsync( relativeTestPath, testResult, usedSyntaxKindsCollector );

            return testResult;
        }

        /// <summary>
        /// Runs the template test with the given path of the source file and asserts that the result of the code transformation matches the expected result.
        /// </summary>
        /// <param name="relativeTestPath">The path of the test source file relative to the project directory.</param>
        /// <returns>The async task.</returns>
        protected async Task AssertTransformedSourceEqualAsync( string relativeTestPath )
        {
            var testResult = await this.RunTemplateTestAsync( relativeTestPath );

            Assert.True( testResult.Success, testResult.ErrorMessage );

            // Compare the "Target" region of the transformed code to the expected output.
            // If the region is not found then compare the complete transformed code.
            var sourceAbsolutePath = Path.Combine( this.ProjectDirectory, relativeTestPath );
            var expectedTransformedPath = Path.Combine( Path.GetDirectoryName( sourceAbsolutePath )!, Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + ".transformed.txt" );
            var expectedTransformedSource = await File.ReadAllTextAsync( expectedTransformedPath );
            var actualTransformedPath = Path.Combine(
                this.ProjectDirectory,
                "obj",
                "transformed",
                Path.GetDirectoryName( relativeTestPath ) ?? "",
                Path.GetFileNameWithoutExtension( relativeTestPath ) + ".transformed.txt" );

            var targetTextSpan = TestSyntaxHelper.FindRegionSpan( testResult.TransformedTargetSyntax, "Target" );
            testResult.AssertTransformedSourceSpanEqual( expectedTransformedSource, targetTextSpan, actualTransformedPath );
        }

        private async Task WriteSyntaxCoverageAsync( string relativeTestPath, TestResult testResult, UsedSyntaxKindsCollector usedSyntaxKindsCollector )
        {
            if ( !usedSyntaxKindsCollector.CollectedSyntaxKinds.Any() )
            {
                return;
            }

            var unsupportedSyntaxKinds = new HashSet<SyntaxKind>();
            var unsupportedDiagnostics = testResult.Diagnostics
                .Where( d => d.Id.Equals( TemplatingDiagnosticDescriptors.LanguageFeatureIsNotSupported.Id, StringComparison.Ordinal ) );

            foreach ( var diagnostic in unsupportedDiagnostics )
            {
                if ( diagnostic.Properties.TryGetValue( TemplatingDiagnosticProperties.SyntaxKind, out var syntaxKindString ) )
                {
                    if ( Enum.TryParse( typeof( SyntaxKind ), syntaxKindString, out var syntaxKind ) )
                    {
                        unsupportedSyntaxKinds.Add( (SyntaxKind) syntaxKind! );
                    }
                }
            }

            var syntaxKindsText = string.Join(
                Environment.NewLine,
                usedSyntaxKindsCollector.CollectedSyntaxKinds
                    .Select( s => unsupportedSyntaxKinds.Contains( s ) ? $"{s}*" : $"{s}" )
                    .OrderBy( s => s ) );

            var filePath = Path.Combine(
                this.ProjectDirectory,
                "obj",
                "SyntaxCover",
                Path.GetDirectoryName( relativeTestPath ) ?? "",
                relativeTestPath + ".txt" );

            Directory.CreateDirectory( Path.GetDirectoryName( filePath ) );
            await File.WriteAllTextAsync( filePath, syntaxKindsText );
        }
    }
}
