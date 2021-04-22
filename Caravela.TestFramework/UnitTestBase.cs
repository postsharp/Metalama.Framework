﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// The base class for integration tests.
    /// </summary>
    /// <remarks>
    /// <para>
    /// You can use <see cref="FromDirectoryAttribute"/> to execute one test method on many test inputs.
    /// This is useful to write only one test method per category of tests.
    /// </para>
    /// </remarks>
    public abstract class UnitTestBase
    {
        protected ITestOutputHelper Logger { get; }

        /// <summary>
        /// Gets the root directory path of the current test project.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value of this property is read from <c>AssemblyMetadataAttribute</c> with <c>Key = "ProjectDirectory"</c>.
        /// </para>
        /// </remarks>
        protected string ProjectDirectory { get; }

        protected string TestInputsDirectory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitTestBase"/> class.
        /// </summary>
        /// <param name="logger">The Xunit logger.</param>
        public UnitTestBase( ITestOutputHelper logger )
        {
            this.Logger = logger;
            this.ProjectDirectory = TestEnvironment.GetProjectDirectory( this.GetType().Assembly );
            this.TestInputsDirectory = TestEnvironment.GetTestInputsDirectory( this.GetType().Assembly );
        }

        protected void WriteDiagnostic( Diagnostic diagnostic )
        {
            this.Logger.WriteLine( diagnostic.ToString() );
        }

        protected abstract TestRunnerBase CreateTestRunner();

        protected void WriteDiagnostics( IEnumerable<Diagnostic> diagnostics )
        {
            foreach ( var diagnostic in diagnostics )
            {
                if ( diagnostic.Severity == DiagnosticSeverity.Error )
                {
                    this.WriteDiagnostic( diagnostic );
                }
            }
        }

        /// <summary>
        /// Runs the template test with the given path of the source file.
        /// </summary>
        /// <param name="relativeTestPath">The path of the test source file relative to the project directory.</param>
        /// <returns>The result of the test execution.</returns>
        protected async Task<TestResult> GetTestResultAsync( string relativeTestPath )
        {
            var testSourceAbsolutePath = Path.Combine( this.TestInputsDirectory, relativeTestPath );

            var testRunner = this.CreateTestRunner();
            var testSource = await File.ReadAllTextAsync( testSourceAbsolutePath );
            var testResult = await testRunner.RunTestAsync( new TestInput( relativeTestPath, testSource ) );

            this.WriteDiagnostics( testResult.Diagnostics );

            return testResult;
        }

        protected async Task AssertTestSuccessful( string relativePathTest )
        {
            var result = await this.GetTestResultAsync( relativePathTest );
            Assert.True( result.Success, result.ErrorMessage );
        }

        // ReSharper disable once ConstantConditionalAccessQualifier
        public static string? NormalizeString( string? s ) => s?.Trim()?.Replace( "\r", "" );

        /// <summary>
        /// Runs the template test with the given path of the source file and asserts that the result of the code transformation matches the expected result.
        /// </summary>
        /// <param name="relativeTestPath">The path of the test source file relative to the project directory.</param>
        /// <returns>The async task.</returns>
        protected async Task AssertTransformedSourceEqualAsync( string relativeTestPath )
        {
            var testResult = await this.GetTestResultAsync( relativeTestPath );

            // Compare the "Target" region of the transformed code to the expected output.
            // If the region is not found then compare the complete transformed code.
            var sourceAbsolutePath = Path.Combine( this.TestInputsDirectory, relativeTestPath );

            var expectedTransformedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + ".transformed.txt" );

            Assert.NotNull( testResult.TransformedTargetSourceText );

            var actualTransformedSourceText = NormalizeString( testResult.TransformedTargetSourceText!.ToString() );

            // Get expectations.
            Assert.True( File.Exists( expectedTransformedPath ), $"File {expectedTransformedPath} does not exist." );
            var expectedNonNormalizedSourceText = await File.ReadAllTextAsync( expectedTransformedPath );
            var expectedTransformedSourceText = NormalizeString( expectedNonNormalizedSourceText );

            // Update the file in obj/transformed if it is different.
            var actualTransformedPath = Path.Combine(
                this.ProjectDirectory,
                "obj",
                "transformed",
                Path.GetDirectoryName( relativeTestPath ) ?? "",
                Path.GetFileNameWithoutExtension( relativeTestPath ) + ".transformed.txt" );

            Directory.CreateDirectory( Path.GetDirectoryName( actualTransformedPath ) );

            var storedTransformedSourceText = File.Exists( actualTransformedPath ) ? NormalizeString( File.ReadAllText( actualTransformedPath ) ) : null;

            if ( expectedTransformedSourceText == actualTransformedSourceText
                 && storedTransformedSourceText != expectedNonNormalizedSourceText )
            {
                // Update the obj\transformed file to the non-normalized expected text, so that future call to update_transformed.txt
                // does not overwrite any whitespace change.
                File.WriteAllText( actualTransformedPath, expectedNonNormalizedSourceText );
            }
            else if ( storedTransformedSourceText == null || storedTransformedSourceText != actualTransformedSourceText )
            {
                File.WriteAllText( actualTransformedPath, actualTransformedSourceText );
            }

            Assert.Equal( expectedTransformedSourceText, actualTransformedSourceText );
        }
    }
}